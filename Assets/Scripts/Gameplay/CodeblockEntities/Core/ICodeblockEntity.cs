using UnityEngine;

namespace CodeblockEntities
{
    public interface ICodeblockEntity
    {
        GameObject GameObject { get; }

        void ModifyHealth(float amount);
        void ModifySpeed(float amount);

        // Extension for future
        void Destroy();
        void Scale(float amount);
    }
}
