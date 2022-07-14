using System;
using System.Collections.Generic;

namespace Pancake.Database
{
    public interface IDataGroup
    {
        public string Title { get; set; }
        public Type Type { get; set; }
        public List<DataEntity> Content { get; set; }
        public void Add(DataEntity entity);
        public void Remove(int key);
        public void CleanUp();
    }
}