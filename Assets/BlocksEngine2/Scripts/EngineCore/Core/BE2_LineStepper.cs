using System.Collections;
using UnityEngine;

using MG_BlocksEngine2.Block;
using MG_BlocksEngine2.Block.Instruction;
using MG_BlocksEngine2.Environment;

namespace MG_BlocksEngine2.Core
{
    public class BE2_LineStepper : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 5f)]
        float _stepDelay = 0.3f;

        bool _isRunning;
        Coroutine _executionCoroutine;
        I_BE2_Block _currentActiveBlock;
        BE2_Line _currentActiveLine;

        public bool IsRunning => _isRunning;
        public float StepDelay => _stepDelay;

        public void Play(BE2_ProgrammingEnv env)
        {
            if (_isRunning) return;
            if (env == null || env.MainLines == null || env.MainLines.Count == 0) return;

            // Clear any lingering visual states
            ClearAllHighlights(env);
            ClearAllShadows(env);

            _isRunning = true;
            _executionCoroutine = StartCoroutine(C_MainLineLoop(env));
        }

        public void Stop()
        {
            _isRunning = false;

            if (_executionCoroutine != null)
            {
                StopCoroutine(_executionCoroutine);
                _executionCoroutine = null;
            }

            SetBlockShadow(_currentActiveBlock, false);
            _currentActiveBlock = null;

            SetLineHighlight(_currentActiveLine, false);
            _currentActiveLine = null;
        }

        IEnumerator C_MainLineLoop(BE2_ProgrammingEnv env)
        {
            while (_isRunning)
            {
                for (int i = 0; i < env.MainLines.Count && _isRunning; i++)
                {
                    BE2_Line line = env.MainLines[i];

                    if (line != null && line.CurrentBlock != null)
                    {
                        yield return C_ExecuteBlock(env, line, line.CurrentBlock);
                    }
                    else
                    {
                        yield return C_WaitOnLine(line);
                    }
                }
            }
        }

        IEnumerator C_ExecuteBlock(BE2_ProgrammingEnv env, BE2_Line line, I_BE2_Block block)
        {
            if (block == null || block.Instruction?.InstructionBase == null)
            {
                yield return new WaitForSeconds(_stepDelay);
                yield break;
            }

            SetLineHighlight(_currentActiveLine, false);
            SetLineHighlight(line, true);
            _currentActiveLine = line;

            SetBlockShadow(block, true);
            _currentActiveBlock = block;

            BE2_InstructionBase instruction = (BE2_InstructionBase)block.Instruction.InstructionBase;
            if (instruction.TargetObject == null)
                instruction.TargetObject = env.TargetObject;
            StepResult result = instruction.ExecuteStep();

            switch (result.Type)
            {
                case StepResultType.Completed:
                    yield return new WaitForSeconds(_stepDelay);
                    break;

                case StepResultType.EnterBody:
                    yield return C_ExecuteBody(env, block, result.BodySectionIndex);
                    yield return new WaitForSeconds(_stepDelay);
                    break;

                case StepResultType.SkipBody:
                    yield return new WaitForSeconds(_stepDelay);
                    break;

                case StepResultType.WaitForSeconds:
                    yield return new WaitForSeconds(result.WaitDuration);
                    yield return new WaitForSeconds(_stepDelay);
                    break;
            }

            SetBlockShadow(block, false);
            _currentActiveBlock = null;

            SetLineHighlight(_currentActiveLine, false);
            _currentActiveLine = null;
        }

        IEnumerator C_ExecuteBody(BE2_ProgrammingEnv env, I_BE2_Block parentBlock, int sectionIndex)
        {
            BE2_BlockSectionBody body = GetBodyFromBlock(parentBlock, sectionIndex);
            if (body == null || body.SubLines == null)
            {
                yield break;
            }

            foreach (BE2_Line subLine in body.SubLines)
            {
                if (!_isRunning) yield break;

                if (subLine != null && subLine.CurrentBlock != null)
                {
                    yield return C_ExecuteBlock(env, subLine, subLine.CurrentBlock);
                }
                else
                {
                    yield return C_WaitOnLine(subLine);
                }
            }
        }

        BE2_BlockSectionBody GetBodyFromBlock(I_BE2_Block block, int sectionIndex)
        {
            if (block?.Layout?.SectionsArray == null || block.Layout.SectionsArray.Length <= sectionIndex)
                return null;

            I_BE2_BlockSection section = block.Layout.SectionsArray[sectionIndex];
            return section?.Body as BE2_BlockSectionBody;
        }

        IEnumerator C_WaitOnLine(BE2_Line line)
        {
            if (line == null)
            {
                yield return new WaitForSeconds(_stepDelay);
                yield break;
            }

            SetLineHighlight(_currentActiveLine, false);
            SetLineHighlight(line, true);
            _currentActiveLine = line;
            yield return new WaitForSeconds(_stepDelay);
            SetLineHighlight(line, false);
            _currentActiveLine = null;
        }

        void SetBlockShadow(I_BE2_Block block, bool active)
        {
            if (block is BE2_Block b)
                b.SetShadowActive(active);
        }

        void SetLineHighlight(BE2_Line line, bool active)
        {
            if (line != null)
                line.SetActiveHighlight(active);
        }

        void ClearAllHighlights(BE2_ProgrammingEnv env)
        {
            if (env?.MainLines == null) return;
            foreach (var line in env.MainLines)
            {
                if (line != null)
                    line.SetActiveHighlight(false);
                if (line?.CurrentBlock != null)
                    ClearHighlightsRecursive(line.CurrentBlock);
            }
        }

        void ClearHighlightsRecursive(I_BE2_Block block)
        {
            if (block == null) return;
            if (block.Layout?.SectionsArray != null)
            {
                foreach (var section in block.Layout.SectionsArray)
                {
                    var body = section?.Body as BE2_BlockSectionBody;
                    if (body?.SubLines != null)
                    {
                        foreach (var subLine in body.SubLines)
                        {
                            if (subLine != null)
                                subLine.SetActiveHighlight(false);
                            if (subLine?.CurrentBlock != null)
                                ClearHighlightsRecursive(subLine.CurrentBlock);
                        }
                    }
                }
            }
        }

        void ClearAllShadows(BE2_ProgrammingEnv env)
        {
            if (env?.MainLines == null) return;
            foreach (var line in env.MainLines)
            {
                if (line?.CurrentBlock != null)
                    ClearShadowsRecursive(line.CurrentBlock);
            }
        }

        void ClearShadowsRecursive(I_BE2_Block block)
        {
            if (block == null) return;
            if (block is BE2_Block b)
                b.SetShadowActive(false);

            if (block.Layout?.SectionsArray != null)
            {
                foreach (var section in block.Layout.SectionsArray)
                {
                    var body = section?.Body as BE2_BlockSectionBody;
                    if (body?.SubLines != null)
                    {
                        foreach (var subLine in body.SubLines)
                        {
                            if (subLine?.CurrentBlock != null)
                                ClearShadowsRecursive(subLine.CurrentBlock);
                        }
                    }
                }
            }
        }
    }
}
