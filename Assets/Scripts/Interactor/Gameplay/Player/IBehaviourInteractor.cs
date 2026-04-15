using UnityEngine;

public interface IBehaviourInteractor
{
    public Transform Transform { get; }
    public IProgrammingEnvironment ProgrammingEnv { get; set; }
}
