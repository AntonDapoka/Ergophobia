using UnityEngine;

public class BehaviourInteractorScript : MonoBehaviour, IBehaviourInteractor
{
    public Transform Transform => transform;
    public IProgrammingEnvironment ProgrammingEnv { get; set; }
}