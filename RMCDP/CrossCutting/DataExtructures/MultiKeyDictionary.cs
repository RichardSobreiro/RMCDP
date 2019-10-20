using System.Collections.Generic;
using System.Threading;

namespace CrossCutting.DataExtructures
{
    public class MultiKeyDictionary<K, L, V>
    {
        internal readonly Dictionary<K, V> baseDictionary = new Dictionary<K, V>();
        internal readonly Dictionary<L, K> subDictionary = new Dictionary<L, K>();
        internal readonly Dictionary<K, L> primaryToSubkeyMapping = new Dictionary<K, L>();
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        // Associate a subkey with a primary key after the primary key
        // and value pair have been added to the underlying dictionary
        public void Associate(L subKey, K primaryKey)
        {
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (!baseDictionary.ContainsKey(primaryKey))
                    throw new KeyNotFoundException(string.Format(
                      "The base dictionary does not contain the key '{0}'", primaryKey));

                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                // Remove the old mapping first
                {
                    readerWriterLock.EnterWriteLock();

                    try
                    {
                        if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                        {
                            subDictionary.Remove(primaryToSubkeyMapping[primaryKey]);
                        }

                        primaryToSubkeyMapping.Remove(primaryKey);
                    }
                    finally
                    {
                        readerWriterLock.ExitWriteLock();
                    }
                }

                subDictionary[subKey] = primaryKey;
                primaryToSubkeyMapping[primaryKey] = subKey;
            }
            finally
            {
                readerWriterLock.ExitUpgradeableReadLock();
            }
        }

        public bool TryGetValue(K primaryKey)
        {
            bool result = false;
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (!baseDictionary.ContainsKey(primaryKey))
                    throw new KeyNotFoundException(string.Format(
                      "The base dictionary does not contain the key '{0}'", primaryKey));

                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    readerWriterLock.EnterWriteLock();

                    try
                    {
                        if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                        {
                            result = true;
                        }
                    }
                    finally
                    {
                        readerWriterLock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                readerWriterLock.ExitUpgradeableReadLock();
            }

            return result;
        }

        // Add a primary key and value pair and associate the given sub key
        public void Add(K primaryKey, L subKey, V val)
        {
            baseDictionary.Add(primaryKey, val);

            Associate(subKey, primaryKey);
        }

        public void Remove(K primaryKey)
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                if (primaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    if (subDictionary.ContainsKey(primaryToSubkeyMapping[primaryKey]))
                    {
                        subDictionary.Remove(primaryToSubkeyMapping[primaryKey]);
                    }

                    primaryToSubkeyMapping.Remove(primaryKey);
                }

                baseDictionary.Remove(primaryKey);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }

        public void Remove(L subKey)
        {
            readerWriterLock.EnterWriteLock();

            try
            {
                baseDictionary.Remove(subDictionary[subKey]);

                primaryToSubkeyMapping.Remove(subDictionary[subKey]);

                subDictionary.Remove(subKey);
            }
            finally
            {
                readerWriterLock.ExitWriteLock();
            }
        }
    }
}
