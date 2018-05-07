using System;
using System.Collections.Generic;
using System.Threading;

namespace MyWebServer
{
    /// <summary>
    /// Implements a collection of Mutexes to perform a short check if files have been locked.
    /// </summary>
    public class MutexMan
    {
        #region OSM Mutex
        private Mutex _OsmFlagMutex = new Mutex();
        private bool _OsmFlag = true;

        /// <summary>
        /// Performs a mutexed check for the bool flag of the .osm file to determine if the file is already in use. If the bool flag is true, it will be set to false to signify the file being used in the next step.
        /// </summary>
        /// <returns>
        /// Returns true if the .osm file is not already in use, which is determined by the respective bool flag being true. Returns false if the state of the bool flag is false.
        /// </returns>
        public bool TryLockOsm()
        {
            bool ret_val = false;

            _OsmFlagMutex.WaitOne();
            if (_OsmFlag)
            {
                _OsmFlag = false;
                ret_val = true;
            }
            _OsmFlagMutex.ReleaseMutex();

            return ret_val;
        }

        /// <summary>
        /// Performs a mutexed state change of the file bool flag from false to true, signifying that the file is no longer in use.
        /// </summary>
        public void ReleaseOsm()
        {
            _OsmFlagMutex.WaitOne();
            _OsmFlag = true;
            _OsmFlagMutex.ReleaseMutex();
        }
        #endregion

        #region Static File Mutex
        private IDictionary<string, Mutex> _StaticFileFlagMutexes = new Dictionary<string, Mutex>();
        private IDictionary<string, bool> _StaticFileFlags = new Dictionary<string, bool>();

        /// <summary>
        /// Performs a mutexed check for the bool flag of the given file to determine if the file is already in use. If the bool flag is true, it will be set to false to signify the file being used in the next step.
        /// </summary>
        /// <param name="filename">
        /// Name of the file that is intended to be locked.
        /// </param>
        /// <returns>
        /// Returns true if the given file is not already in use, which is determined by the respective bool flag being true. Returns false if the state of the bool flag is false.
        /// </returns>
        public bool TryLockFile(string filename)
        {
            bool ret_val = false;

            if (filename != null)
            {
                if (_StaticFileFlagMutexes.TryGetValue(filename, out Mutex FileFlagMutex))
                {
                    _StaticFileFlagMutexes[filename].WaitOne();

                    if (_StaticFileFlags[filename])
                    {
                        _StaticFileFlags[filename] = false;
                        ret_val = true;
                    }

                    _StaticFileFlagMutexes[filename].ReleaseMutex();
                }
                else
                {
                    _StaticFileFlagMutexes.Add(filename, new Mutex());

                    _StaticFileFlagMutexes[filename].WaitOne();

                    if (_StaticFileFlags.TryGetValue(filename, out bool FileFlag))
                    {
                        _StaticFileFlags[filename] = false;
                        ret_val = true;
                    }
                    else
                    {
                        _StaticFileFlags.Add(filename, false);
                        ret_val = true;
                    }

                    _StaticFileFlagMutexes[filename].ReleaseMutex();
                }
            }

            return ret_val;
        }

        /// <summary>
        /// Performs a mutexed state change of the file bool flag from false to true, signifying that the file is no longer in use.
        /// </summary>
        /// <param name="filename">
        /// Name of the file that is intended to be released.
        /// </param>
        public void ReleaseFile(string filename)
        {
            try
            {
                _StaticFileFlagMutexes[filename].WaitOne();

                _StaticFileFlags[filename] = true;

                _StaticFileFlagMutexes[filename].ReleaseMutex();
            }
            catch
            {
                Console.WriteLine("MutexMan: Tried to release Mutex that wasn't locked.");
            }
        }

        #endregion
    }
}
