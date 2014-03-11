using System;
using System.Web;

namespace Astrila.Eq2Img
{
	public class Eq2ImgUrlMapperModule : IHttpModule
	{


		HttpApplication _Application;
		EventHandler _ApplicationBeginRequestEventHandler; 
		public virtual void Init(System.Web.HttpApplication application)
		{
			_Application = application;
			_ApplicationBeginRequestEventHandler = new EventHandler(Application_BeginRequest);
			_Application.BeginRequest += _ApplicationBeginRequestEventHandler;
		}

		private void Application_BeginRequest(Object sender, EventArgs e)
		{
			HttpApplication thisApplication = (HttpApplication)sender;
			string requestUrl = thisApplication.Request.Url.ToString();
			int eqStart = requestUrl.IndexOf("$");
			int eqEnd = requestUrl.IndexOf("$.aspx");
			if (eqEnd > eqStart)
			{
				//transfer the request to http handler
				thisApplication.Context.RewritePath("~/ShowEq.ashx?" + requestUrl.Substring(eqStart+1,eqEnd-eqStart-1));
			}
			else {}; //ignore this request
		}

		public virtual void Dispose()
		{
			_Application.BeginRequest -= _ApplicationBeginRequestEventHandler;
			_Application = null;
		}
	}
}
