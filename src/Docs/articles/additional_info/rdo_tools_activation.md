---
uid: rdo_tools_activation
---

# RDO.Tools Activation

RDO.Tools must be activated/licensed after installation. You can manage the license via RDO.Tools License Window:

![image](/images/RdoToolsLicenseWindow.jpg)

Show this window by clicking menu "Tools" -> "RDO.Tools License..." in Visual Studio:

![image](/images/RdoToolsLicenseMenu.jpg)

If you're using Visual Studio Community Edition, you can use RDO.Tools for free (only registration required); if you're using other editions of Visual Studio, you need a subscription license or a trial license for one month. Please refer to <xref:rdo_tools_license> and <xref:manage_registration_subscription> for details.

Once you're properly registered/subscribed, you can activate RDO.Tools by login using your registered email address and password, and RDO.Net will be licensed:

![image](/images/RdoToolsLicensed.jpg)

Login in the license window needs HTTPS access to the license server. If you cannot access the license server, for example, your development machine is behind the corporate firewall, you can activate RDO.Tools manually via "Offline" tab of the license window:

![image](/images/RdoToolsLicenseOffline.jpg)

>[!WARNING]
>Normally the local license displayed in license window is synchronized with the license server automatically. However, under certain circumstances, for example, when RDO.Tools is activated via offline, the local license can be invalid. In this case, you're responsible to keep these two synchronized manually, to ensure no license violation.
