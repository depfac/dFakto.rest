using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.Specification;

namespace dFakto.Rest.SampleApi.Tools
{
    public enum SampleEnum
    {
        First, Second, Third
    }
    
    public class MySampleValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
            
        public DateTime SomeDate { get; set; }
            
        public long SomeLong { get; set; }
        
        public SampleEnum SomeEnum { get; set; }
    }
    
    public class SampleRepository
    {
        private Random _random = new Random();
        
        private List<MySampleValue> _store = new List<MySampleValue>();

        public SampleRepository()
        {
            _store.AddRange(GetValues(1000));
        }
        
        public IEnumerable<MySampleValue> List(ISpecification<MySampleValue> specification)
        {
            return specification.Evaluate(_store);
        }
        
        public MySampleValue GetById(int id)
        {
            return _store.FirstOrDefault(x => x.Id == id);
        }
        
        private IEnumerable<MySampleValue> GetValues(int max)
        {
            int i = 0;
            while (i < max)
            {
                i++;
                yield return new MySampleValue
                {
                    Id = i,Value = "Value" + i,
                    SomeDate = DateTime.Now.AddDays(_random.Next(-1000,1000)),
                    SomeLong = _random.Next(10000),
                    SomeEnum = (SampleEnum)_random.Next(0,3)
                };
            }
        }

        public MySampleValue Create(MySampleValue value)
        {
            value.Id = _store.Count + 1;
            value.Value = "Value" + value.Id;
            _store.Add(value);
            return value;
        }

        public void DeleteById(int id)
        {
            var r = GetById(id);
            _store.Remove(r);
        }

        public void Update(int id, MySampleValue value)
        {
            var r = GetById(id);
            r.Value = value.Value;
            r.SomeDate = value.SomeDate;
            r.SomeLong = value.SomeLong;
        }
    }
}