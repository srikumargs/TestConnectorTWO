namespace DBTool
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlServerCe;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DBTool.Internal;

    /// <summary>
    /// Class to perform migrations
    /// </summary>
    /// <remarks>
    /// Note that there is a tension between chaining version upgrades
    /// and allowing for consolidated upgrades.
    /// 
    /// DB version upgrades are expected to happen but not happen a lot. We are
    /// only talking about changes to the configuration database and possibly new databases
    /// it is expected that the queue and log databases are backed up but not upgraded.
    /// Note that script files are expected to carry along with the database
    /// </remarks>
    internal class Migration
    {
        /// <summary>
        /// Do a database schema and file migration.
        /// This is the main entry point
        /// </summary>
        /// <param name="priorVersion"></param>
        /// <param name="priorVersionBackupDir"></param>
        /// <param name="currentVersion"></param>
        /// <param name="instanceDataDir"></param>
        /// <param name="writeLine"></param>
        /// <returns></returns>
        /// <remarks>
        /// Backups must have been done already
        /// </remarks>
        internal bool Migrate(
                        string priorVersion,
                        string priorVersionBackupDir,
                        string currentVersion,
                        string instanceDataDir,
                        Action<string> writeLine)
        {
            bool retval = false;

            //Get versions and validate parameters
            //TODO: do we need checks on security permissions for directories?
            Version priorDbVersion;
            Version currentDbVersion;

            bool parametersValidated = Version.TryParse(priorVersion, out priorDbVersion);
            parametersValidated &= Version.TryParse(currentVersion, out currentDbVersion);
            parametersValidated &= Directory.Exists(priorVersionBackupDir);
            parametersValidated &= Directory.Exists(instanceDataDir);
            parametersValidated &= (writeLine != null);

            if (parametersValidated)
            {
                MigrationInfo info = new MigrationInfo(priorDbVersion,
                                                        priorVersionBackupDir,
                                                        currentDbVersion,
                                                        instanceDataDir,
                                                        writeLine);
                retval = Migrate(info);
            }
            return retval;
        }

        /// <summary>
        /// Do the actual migration
        /// We have verified the parameters at this point.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <remarks>
        /// We expect that all parameters have had basic validation at this point.
        /// </remarks>
        private bool Migrate(MigrationInfo info)
        {
            //since we are going to try to do a upgrade, we are only successful if the plan succeeds
            bool retval = false;

            MigrationPlan doMigration = GetMigrationPlan(info.PriorVersion, info.CurrentVersion, info.WriteLine);
            if (doMigration != null)
            {
                try
                {
                    retval = doMigration(info);
                }
                catch (Exception ex)
                {
                    //TODO: look at changing this some feels odd.
                    info.WriteLine(String.Format("Exception encounterd while migrating databases: {0}", ex.ToString()));
                    info.WriteLine("Attempting to replace with baseline version.");
                    try
                    {
                        ReplaceAllPlan(info);
                    }
                    catch (Exception innerEx)
                    {
                        info.WriteLine(String.Format("Exception Encounterd: {0}", innerEx.ToString()));
                        info.WriteLine("Unable to update database files");
                    }

                    throw;
                }
            }
            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="priorVersion"></param>
        /// <param name="currentVersion"></param>
        /// <returns></returns>
        private MigrationPlan GetMigrationPlan(Version priorVersion, Version currentVersion, Action<string> writeLine)
        {
            //remember we are dealing "interval" logic
            MigrationPlan plan = null;

            List<Version> changes = _schemaChangeMarkers.Where((v) => (priorVersion < v && v <= currentVersion)).ToList();

            Version minimumVersionSchemaChangeVersion = _schemaChangeMarkers[0];
            if (priorVersion < minimumVersionSchemaChangeVersion)
            {
                //support for min version to upgrade from or unknown version
                //should never happen - upgrading from version that is before known versions.
                //This is an error - Consider throwing invalidOp expectation
                plan = ReplaceAllPlan;
                writeLine("Unregognized database version detected, replacing with installed version.");
            }
            else if (currentVersion < priorVersion)
            {
                //TODO: do we even allow current <= prior?

                //version is moving backwards, maybe someone trying to move backwards from a beta?
                //we either error, do nothing, or replace it all, for now replace everything
                plan = ReplaceAllPlan;
                writeLine("Database version downgrade detected, replacing with installed version.");
            }
            else if (changes.Count == 0)
            {
                //(currentVersion == priorVersion)
                //(priorVersion >= mostRecentSchemaChangeVersion)
                //reinstalling the same version or no schema changes to current version from prior.
                //update version tag, and always upgraded files only.
                //leave the schema configuration alone.

                //NOTE: if we change the upgraded file list version to version this must change.
                plan = ReplaceAlwaysUpgradedFilesPlan;
                writeLine("No database version change detected, ConfigurationStore unchanged.");
            }
            else
            {
                //we are actually crossing at least one schema change.
                //TODO: consider data structure for version upgrades, run thru table maybe
                //can we ever consolidate ranges? possibly any changes in the middle would be broken unless they were compensating changes ?

                //we really only want the highest numbered change.
                string versionsString = "1.0.50304.1";
                if (changes.Contains(new Version(versionsString)))
                {
                    plan = UpgradeTo1_0_50304_1;
                    writeLine(string.Format("Database version change detected, updateing ConfigurationStore to {0}.", versionsString));
                }

                //apply changes in order.
                //versionsString = "v.Next";
                //if (changes.Contains(new Version(versionsString)))
                //{
                //    //UpgradeVNext must include prior upgrades.
                //    //may end up being just apply part of priors
                //    plan = UpgradeVNext;
                //}
            }

            return plan;
        }

        #region ReplaceAllFilesPlan
        /// <summary>
        /// Replace every file, this is the "make it work" fall back.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <remarks>
        /// This needs to just pull all files from the baseline.
        /// </remarks>
        private bool ReplaceAllPlan(MigrationInfo info)
        {
            bool retval = true;

            try
            {
                var fileList = GetAllBaselineFiles(info.InstanceDataDir);

                foreach (string fullFileName in fileList)
                    ReplaceFile(fullFileName, info.InstanceDataDir);

                info.WriteLine("All database files updated to installed version.");
                info.WriteLine("Any existing connections to the cloud were unable to be migrated and will need to be reconfigured.");
                info.WriteLine("A copy of the prior version is availble in the backups directory.");

            }
            catch (Exception ex)
            {
                //yeah we are are only re-throwing and the retval assign is not needed or fully correct,
                //but it makes a handy place to but a breakpoint and see what is going on while debugging.
                retval = string.IsNullOrEmpty(ex.Message);
                throw;
            }

            return retval;
        }

        #endregion ReplaceAllFilesPlan

        #region ReplaceAllButFilesToUpdatePlan
        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <remarks>
        /// This needs to pull all files from the baseline other then the ones we commonly upgrade.
        /// Commonly upgraded files will vary with time possibly.
        /// </remarks>
        private bool ReplaceAlwaysUpgradedFilesPlan(MigrationInfo info)
        {
            bool retval = true;

            try
            {
                //get all the files in the baseline other then those that we want to upgrade, or leave intact.
                var files = GetAllBaselineFiles(info.InstanceDataDir).Where((f) => !FileNamesToUpgradeIfPossible().Contains(Path.GetFileName(f)));

                foreach (string fileName in files)
                    ReplaceFile(fileName, info.InstanceDataDir);

                info.WriteLine("Queue and Log database files updated to the installed version.");
                info.WriteLine("A copy of the prior version is availble in the backups directory.");
            }
            catch (Exception ex)
            {
                //yeah we are are only re-throwing and the retval assign is not needed or fully correct,
                //but it makes a handy place to but a breakpoint and see what is going on while debugging.
                retval = string.IsNullOrEmpty(ex.Message);
                throw;
            }

            return retval;
        }

        private IEnumerable<string> GetAllBaselineFiles(string instanceDataDir)
        {
            string blankDbPath = BlankDBFilePath(instanceDataDir);
            var fileList = Directory.EnumerateFiles(blankDbPath, "*", SearchOption.TopDirectoryOnly);
            return fileList;
        }

        private string BlankDBFilePath(string instanceDataDir)
        {
            return Path.Combine(instanceDataDir, "Baseline");
        }

        private IEnumerable<String> FileNamesToUpgradeIfPossible()
        {
            //TODO: look at not hard coding, consider version specific aspects
            string[] files = new string[] { "ConfigurationStore.sdf" };
            return files;
        }

        private void ReplaceFile(string fullSourceFilePath, string fileDestinationDir)
        {
            string fileName = Path.GetFileName(fullSourceFilePath);
            string fullPathOldFile = Path.Combine(fileDestinationDir, fileName);
            string fullPathNewFile = fullSourceFilePath;

            //TODO: replace with copy and overwrite?
            if (File.Exists(fullPathOldFile))
            {
                File.Delete(fullPathOldFile);
            }
            File.Copy(fullSourceFilePath, fullPathOldFile);
        }

        #endregion ReplaceAllButFilesToUpdate

        #region UpgradeTo1_0_50304_1
        private bool UpgradeTo1_0_50304_1(MigrationInfo info)
        {
            string versionString = "1.0.50304.1";

            //NOTE: WipName MUST NOT exist in the baseline. 
            File.Copy(info.ConfigFile, info.WipFile, true);
            ReplaceAllPlan(info);

            //from here out we have a working configuration if something blows up.
            SqlCeConnectionStringBuilder builder = new SqlCeConnectionStringBuilder();
            builder.DataSource = info.WipFile;

            using (SqlCeConnection conn = new SqlCeConnection(builder.ToString()))
            {
                conn.Open();
                ApplySchemaChange1_0_50304_1(conn, info.WriteLine);
            }

            File.Copy(info.WipFile, info.ConfigFile, true);
            File.Delete(info.WipFile);
            info.WriteLine(string.Format("ConfigurationStore updated to version {0}.", versionString));
            return true;
        }

        /// <summary>
        /// Actually change the schema 
        /// </summary>
        /// <param name="connection"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Local SQL only so no injection possible")]
        private void ApplySchemaChange1_0_50304_1(SqlCeConnection connection, Action<String> writeLine)
        {
            string versionString = "1.0.50304.1";

            //TODO: Move to only in memory transforms and dump to latest schema if we are not ok with losing "not null"
            //unless we do only in memory transforms up to the latest version then right
            //we are going to have schema drift on null vs. not null columns

            string[] sql = new string[] 
            {
                @"ALTER TABLE [PremiseConfigurations] ADD [SiteAddress] nvarchar(4000) NULL",
                @"ALTER TABLE [PremiseConfigurations] DROP COLUMN [CloudEndpoint]",
                @"UPDATE [PremiseConfigurations]
                  SET [SiteAddress2] = 'https://www.sageconstructionanywhere.com/' "
            };

            //            string[] sql = new string[] 
            //            {
            //                @"ALTER TABLE [PremiseConfigurations] ADD [SiteAddress2] nvarchar(4000) NULL",
            //                @"ALTER TABLE [PremiseConfigurations] DROP COLUMN [SiteAddress]",
            //                @"UPDATE [PremiseConfigurations]
            //                  SET [SiteAddress2] = 'https://www.sageconstructionanywhere.com/zoiks' "
            //            };

            foreach (string s in sql)
            {
                using (SqlCeCommand cmd = new SqlCeCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = s;
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();
                }
            }
            writeLine(string.Format("ConfigurationStore schema updated to  {0}.", versionString));
        }
        #endregion UpgradeTo1_0_50304_1

        //centralize the plan return so we have the option to
        //have chained or all in one upgrades from version to version.

        //it is a version upgrade though we may not actually change anything

        //NOTE: uninstall and then reinstall is NOT upgraded its moved to a backup folder and new bits applied
        //code for it is in the connector startup.
        //NOTE: We do NOT have a file version stamp. It could be nice but logic does not require it.

        /// <summary>
        /// List of schema changed versions.
        /// List must be in order.
        /// Current pattern needs version string 
        ///  * here 
        ///  * in the two supporting upgrade functions.
        ///  * In the sequencing function.
        ///  
        /// For some reason I cant put my finger on setting up for chaining vs 
        /// discreate upgrade path concerns me even with the ugly over lap between discreete upgrades.
        /// I can't put my finger on why I dont like it though, seem like it should work...
        /// Left rough untill we have a few upgrades and maybe it becomes more clear.
        /// Current approa
        /// </summary>
        static private List<Version> _schemaChangeMarkers = new List<Version>{
            new Version("0.1.0.0"),   
            new Version("1.0.50304.1")
            };

        /// <summary>
        /// Standard signature for the plans
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private delegate bool MigrationPlan(MigrationInfo info);

        /// <summary>
        /// Storage for the general info
        /// </summary>
        private class MigrationInfo
        {
            public MigrationInfo(
                Version priorVersion,
                string priorVersionBackupDir,
                Version currentVersion,
                string instanceDataDir,
                Action<string> writeLine)
            {
                PriorVersion = priorVersion;
                PriorVersionBackupDir = priorVersionBackupDir;
                CurrentVersion = currentVersion;
                InstanceDataDir = instanceDataDir;
                WriteLine = writeLine;

                //TODO: use temp subdir for WIP.
                //not 100% sure these should even be in this class.
                //will need an array of wip files if we move to multi upgrade
                //funky name to we do not get collisions with the baseline files
                WipFile = Path.Combine(instanceDataDir, "WipFile0001.sdf");
                ConfigFile = Path.Combine(instanceDataDir, "ConfigurationStore.sdf");
            }

            public Version PriorVersion { get; protected set; }
            public string PriorVersionBackupDir { get; protected set; }
            public Version CurrentVersion { get; protected set; }
            public string InstanceDataDir { get; protected set; }
            public Action<string> WriteLine { get; protected set; }
            public string WipFile { get; protected set; }
            public string ConfigFile { get; protected set; }
        }
    }
}

