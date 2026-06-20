using System.Collections.Generic;

namespace CodeblockEntities
{
    public interface ICodeblockEntityManager
    {
        CodeblockEntityType EntityType { get; }
        IReadOnlyList<ICodeblockEntity> GetEntities();
        void Refresh();
    }
}
