using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHU_First_Run_Assistant
{
    public class LocalSystemInterface
    {
        public LocalSystemInterface()
        {

        }
        public void UpdatePolicy()
        {

            //build gpupdate process for hidden window launch
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "gpupdate";
            process.StartInfo = startInfo;
            
            //launch gpupdate process
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
        }           // Causes the local machine to start a GPUpdate in order to grab new machine and user policies
    }
}
