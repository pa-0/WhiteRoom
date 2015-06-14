using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WhiteRoom")]
[assembly: AssemblyDescription("WhiteRoom is a full screen, distraction free, writing environment. WhiteRoom is a fork; an updated and maintained version of DarkRoom (or Dark Room W). Unlike standard word processors that focus on features, DarkRoom is just about you and your text. Basically, DarkRoom is a clone of the original WriteRoom that is an OS X exclusive application.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("WhiteRoom")]
[assembly: AssemblyCopyright("Copyright (C) Joe DF (2015), Jeffrey Fuller (2010)")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d480df88-eea0-446d-bbed-fb6d614346da")]

/* from: https://github.com/ahkscript/ASPDM/blob/master/Specifications/Guidelines.md#autohotkey-flavored-semantic-versioning
AutoHotkey-Flavored Semantic Versioning

Given a version number MAJOR.MINOR.FEATURE.PATCH, increment the:

    MAJOR version when major breaking changes are made,
    MINOR version when minor breaking changes that do not affect a majority of users,
    FEATURE version when backwards-compatible (or breaking in rare cases) new features are added, and
    PATCH version when mere bug fixes are made. A release with a significant number of bug fixes can lead into a 'FEATURE' version bump.

Additional labels for pre-release and build metadata are available as extensions to the given format.

Pre-release/stable software version tag format: 0.BUILD[TAG]-PHASE
*/
[assembly: AssemblyVersion("0.9.2.2")]
[assembly: AssemblyFileVersion("0.9.2.2")]
