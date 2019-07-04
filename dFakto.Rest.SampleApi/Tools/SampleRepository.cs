using System;
using System.Collections.Generic;
using System.Linq;
using dFakto.Rest.SampleApi.Controllers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal;

namespace dFakto.Rest.SampleApi.Tools
{
    public class MySampleValue
    {
        public int Id { get; set; }
        public string Value { get; set; }
            
        public DateTime SomeDate { get; set; }
            
        public long SomeLong { get; set; }
    }
    
    public class SampleRepository
    {
        private Random _random = new Random();
        
        private List<MySampleValue> _store = new List<MySampleValue>();

        public SampleRepository()
        {
            _store.AddRange(GetValues(1000));
        }
        
        public IEnumerable<MySampleValue> GetValues(int index, int max, string[] sort = null)
        {
            IOrderedEnumerable<MySampleValue> str = _store.OrderBy(x => x.Value);
            if (sort != null)
            {
                for (int i = 0; i < sort.Length; i++)
                {
                    var s = sort[i];
                    bool desc = false;
                    if (s.StartsWith('-'))
                    {
                        desc = true;
                        s = s.Substring(1);
                    }
                    switch (s)
                    {
                        case "date":
                            str = i == 0
                                ? (desc ? str.OrderByDescending(x => x.SomeDate) : str.OrderBy(x => x.SomeDate))
                                : (desc ? str.ThenByDescending(x => x.SomeDate) : str.ThenBy(x => x.SomeDate));
                            break;
                        case "long":
                            str = i == 0
                                ? (desc ? str.OrderByDescending(x => x.SomeLong) : str.OrderBy(x => x.SomeLong))
                                : (desc ? str.ThenByDescending(x => x.SomeLong) : str.ThenBy(x => x.SomeLong));
                            break;
                        default:
                            str = i == 0
                                ? (desc ? str.OrderByDescending(x => x.Value) : str.OrderBy(x => x.Value))
                                : (desc ? str.ThenByDescending(x => x.Value) : str.ThenBy(x => x.Value));
                            break;
                    }
                }
            }
            
            return str.Skip(index).TakeWhile((x,y) => y < max);
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
                yield return new MySampleValue{Id = i,Value = "Value" + i,SomeDate = DateTime.Now.AddDays(_random.Next(-1000,1000)),SomeLong = _random.Next(10000)};
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