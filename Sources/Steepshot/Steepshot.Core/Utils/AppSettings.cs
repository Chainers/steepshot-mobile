using Akavache;
using Ninject;
using System.Reactive.Linq;
using System.Collections.Generic;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static StandardKernel Container { get; set; }

        public static bool IsDev
        {
            get
            {
                try
                {
                    return BlobCache.UserAccount.GetObject<bool>(Constants.IsDevKey).Wait();
                }
                catch(KeyNotFoundException)
                {
                    return false;
                }
            }
            set
            {
                BlobCache.UserAccount.InsertObject(Constants.IsDevKey, value).Wait();
            }
        }
    }
}
