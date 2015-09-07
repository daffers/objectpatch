namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Lib;

    public class Data
    {
        public string Header { get; set; }
        public int Rating { get; set; }

        public ChildData Child { get; set; }

        public IEnumerable<string> Custom { get; set; }
    }

    public class ChildData
    {
        public string Name { get; set; }
    }

    public class EditChildDataRequest : PatchRequestBase<ChildData>
    {
        public string Name
        {
            set
            {
                RecordUpdate(x => x.Name, value);
            }
        }
    }

    public class EditDataRequest : PatchRequestBase<Data>
    {
        public string Header
        {
            set
            {
                RecordUpdate(x => x.Header, value);
            }
        }

        public int Rating
        {
            set
            {
                RecordUpdate(x => x.Rating, value);
            }
        }

        public EditChildDataRequest Child
        {
            set
            {
                RecordChildPropertyUpdate(parent => parent.Child, value);
            }
        }

        public IEnumerable<string> Custom
        {
            set
            {
                Func<IEnumerable<string>, Data, bool> check =
                    (enumerable, data) =>
                    {
                        var result = enumerable.Except(data.Custom).Any()
                               || data.Custom.Except(enumerable).Any();
                        return result;
                    };

                Action<Data, IEnumerable<string>> setProp =
                    (data, newVal) => data.Custom = newVal;

                RecordUpdateWithCustomEqualityCheck((data, val) => check(value, data), value, setProp);
            }
        }
    }
}
