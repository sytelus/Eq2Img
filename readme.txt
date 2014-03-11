Got questions?

1. How do I integrate this code with my app in 10 minutes?
2. What else this folder contains?
3. How do I get licensing and totally free support?





1. How do I integrate this code with my app in 10 minutes?
--------------------------------------------------------------
Simply drop the files in folder OnlyRequiredFiles into your ASP.Net project and drop the MimeTex.DLL in bin folder. Edit your default.aspx page and put this line somewhere in Page_Load:

Astrila.Eq2Img.ShowEq.HandleEquationQueries();




2. What else this folder contains?
--------------------------------------------------------------
This folder contains source code which can render any given mathematical equation string that conforms to TeX/LaTeX standards in to an GIF file. 

The Eq2Img folder contains complete ASP.Net project that you can deploy in to IIS virtual folder and fire a query like this:

http://localhost/eq2img/ShowEq.ashx?f(x) = x^2

The Eq2ImgWinForms folder contains complete user-friendly WinForms program that lets you type in equations and experiment.

The MimeTeX folder contains complete VC++ project that compiles code in to Win32 DLL. Both of above .Net project calls in to MimeTeX DLL to do the real work. You can know more about MimeTeX at http://www.forkosh.com/mimetex.html.

Finally, the OnlyRequiredFiles folder contains only those files that you can simply drop in to your ASP.Net web project and recompile to add equation rendering capability in your app. See previous section for more info.




3. How do I get licensing and totally free support?
--------------------------------------------------------------
All above code except MimeTeX is developed by Shital Shah and is distributed absolutely free with source code as ANYWAY you like it. MimeTeX is developed by Jon Forkosh and is distributed under GPL license. 

I will answer to support questions absolutely free to help you use this code in your applications at support@astrila.com, however NO timeliness of responses or continuation of this free support in future is guaranteed.