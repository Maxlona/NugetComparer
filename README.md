**Nuget Comparer** is an awesome utility to scan and report vulnerable nuggets in VS.net.
From my own first-hand experience with Visual Studio, it doesn't do an excellent job reporting vulnerable nuggets, or knowing why a nuget was flagged as a vulnerable.

This tool was a prototype for an active nuget scanner which will give you few important pieces of information:

1- **Why a nugget flagged as vulnerable?** Vulnerablility details can be viewed on the nuget.org web-pages on why a nuget was flagged as a vulnerability, and wheather or not an update is available, with that said, 
a nugget might be flagged as vulnerable for several reasons:

- Known Security Vulnerabilities: The package might contain a security vulnerability identified by organizations such as the Common Vulnerabilities and Exposures (CVE) program. These vulnerabilities could include issues like injection attacks, authentication bypasses, or insecure configurations.

- Outdated Dependencies: If a NuGet package relies on other libraries with known vulnerabilities, it can inherit these risks even if the code within the package itself is secure. Package maintainers might update their packages to address these, but older versions may still be flagged.

- Improper Security Practices: Sometimes, a package’s code has insecure coding practices, like weak cryptography or lack of input validation, making it susceptible to attacks. These issues may not have been flagged during the initial release but are detected over time.

- Package Management Alerts: NuGet packages are often monitored by security tools like GitHub Dependabot or Azure DevOps, which continually scan for vulnerabilities. If a vulnerability is discovered in any version of a package, these tools can flag it automatically.

- End-of-Life (EOL) Status: A package may no longer be maintained or supported by its authors, meaning it won't receive security patches. Security scanners may flag these packages because they won't be updated for newly discovered vulnerabilities.


2- **Is there an update available?** The tool will give you the last version recommended to install, based on the nuget.org release versions available.

This is a prototype, and I am working on updating it to have a nice UI && .NET Core nuggets scanning Support, 
Cheers,

<img src="https://github.com/Maxlona/NugetComparer/blob/master/NugetComparer/Screenshot.png" />
