using Simplic.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simplic.Icon.Service
{
    /// <summary>
    /// Cacheable icon
    /// </summary>
    internal class IconCache : ICacheable
    {
        /// <summary>
        /// Gets the icon cache-key
        /// </summary>
        public const string CacheKey = "IconCache";

        public string Key => throw new NotImplementedException();

        /// <summary>
        /// Gets or sets a list of cached icons
        /// </summary>
        public IList<Icon> Icons { get; internal set; } = new List<Icon>();

        public void OnRemove()
        {

        }
    }
}
