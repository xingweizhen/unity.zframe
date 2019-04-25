using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZFrame.Asset
{
    /// <summary>
    /// Git 工具
    /// </summary>
    public class GitTools
    {
        private static Regex R_VER = new Regex("v(?<VER>.*?)(-|$)");
        private static Regex R_UPDATE = new Regex("-(?<UP>.*?)-");
        private static Regex R_BRANCH = new Regex("\\* (?<BR>.*?)$", RegexOptions.Multiline);

        /// <summary>
        /// 获取当前分支版本信息
        /// </summary>
        /// <returns></returns>
        public static string getGitVersion()
        {
            try
            {
                string verStr = Exec.DoExec("git", "describe");
                LogMgr.D("GITTOOLS verStr:" + verStr);
                Match m = R_UPDATE.Match(verStr);
                string upStr = "";
                if (m != null
                    && m.Groups != null
                    && m.Groups.Count > 0)
                {
                    upStr = m.Groups["UP"].Value;
                    if (upStr == null || "".Equals(upStr)) upStr = "0";
                }

                string serStr = verStr.IndexOf('-') > 0 ? verStr.Substring(verStr.LastIndexOf('-') + 1).Trim().Trim('\n') : "";

                String vStr = "";
                m = R_VER.Match(verStr);
                if(m != null 
                    && m.Groups != null
                    && m.Groups.Count > 0)
                {
                    vStr = m.Groups["VER"].Value;
                }
                if (vStr == null || "".Equals(vStr))
                {
                    return "unknow";
                }

                return (serStr == null || "".Equals(serStr)) ? 
                    string.Format("v{0}.{1}", vStr, upStr) : string.Format("v{0}.{1}_{2}", vStr, upStr, serStr);
            }
            catch (Exception ex)
            {
                LogMgr.E("git get version fail:" + ex.Message);
                return "unknow";
            }
        }

        public static string getLastCommit()
        {
            try {
                return Exec.DoExec("git", "log --oneline -1 --pretty=format:\"%h\"");
            } catch (Exception ex) {
                LogMgr.E("git get last commit fail:" + ex.Message);
                return "unknow";
            }
        }

        public static string getBranch()
        {
            try {
                string branchStr = Exec.DoExec("git", "branch");
                LogMgr.D("GITTOOLS branchStr:" + branchStr);
                Match m = R_BRANCH.Match(branchStr);
                if (m != null
                    && m.Groups != null
                    && m.Groups.Count > 0)
                {
                    return m.Groups["BR"].Value;
                }
                return branchStr;
            }
            catch (Exception ex)
            {
                LogMgr.E("git get branch fail:" + ex.Message);
                return "unknow";
            }
        }

        /// <summary>
        /// 版本信息
        /// </summary>
        /// <returns>格式为 branch_ver</returns>
        public static string getVerInfo()
        {
            return getBranch()+"_"+getGitVersion();
        }
    }
}
