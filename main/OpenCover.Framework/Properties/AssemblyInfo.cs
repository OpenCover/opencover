using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using log4net.Config;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OpenCover.Framework")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("OpenCover.Framework")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("53accd0b-90ce-4ffe-8dd1-69fdaea96574")]

[assembly: InternalsVisibleTo("OpenCover.Test, PublicKey=" +
                              "002400000480000094000000060200000024000052534131000400000100010077656a2e0ca129"+
                              "31d0f7180cef5801601541e64427a1d73210476112050594a96423051bb50b25026507b2da2c66" +
                              "c66b8a7782f38cbd4240ef3a5c53a6014fb73c99628d40825864b8d1b2e63bfbad678a424c111b" +
                              "14c5620fbc55991499b4d51f0690fd24c9a0406132aa624273c4f685d6e0f3bf78871ca03f985b" +
                              "cc9cbdbd")]

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=" +
                              "0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99" +
                              "c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654" +
                              "753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46" +
                              "ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484c" +
                              "f7045cc7")]

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
