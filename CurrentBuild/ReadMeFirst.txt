== Eudora.Net Installer Readme
== Installation will fail if you skip this!
================================================

New Step 0 -- 6/6/2024
This step is only necessary if a version of Eudora.Net from the old repo has been run on the target PC (the one being installed to)

In the Windows Taskbar, click Start.
Choose Settings (gear icon)
Choose Apps
Choose Apps & features
Scroll down the list. If you find any prior versions of Eudora.Net, select and choose Uninstall.

Now you can follow the remaining steps. Step 0 should only need to be done one time; thereafter, a version update will install as normal.

----------------------------------------------
This pre-release build is signed with a development key. This key
will need to be installed before the msix can run successfully.

Make sure the user copies both the msix and Eudora.Net.Installer_TemporaryKey.pfx


To install the key, follow these steps:

1. Right-click the msix. Click Properties in the menu.

2. Click the Digital Signatures tab.

3. Select Eudora.Net from the list of signatures.

4. Click Details button.

5. In the window that opens, click View Certificate.

6. In the window that opens, click Install Certificate.

7. The Certificate Import Wizard will now open. Select Local Machine, 
then click Next.

8. Select "place all certificates in the following store" and click Browse.

9. A small window will open. Scroll down and select Trusted People, then click OK.
The small window will then close.

10. Click Next, then Finish. You should be notified that the certificate
import was successful.

11. Close the details and properties windows that are still open.

12. Now you can double-click the msix to begin the installation of Eudora.Net.


