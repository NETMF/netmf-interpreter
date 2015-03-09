using System;
using System.Windows.Forms.Design;
//If you are misssing ParentControlDesigner, then don't forget that you need a reference in
//this project to System.Design

namespace Gui.Wizard
{
	/// <summary>
	/// 
	/// </summary>
	public class InfoContainerDesigner : ParentControlDesigner
	{

//		/// <summary>
//		/// Prevents the grid from being drawn on the Wizard
//		/// </summary>
//		protected override bool DrawGrid
//		{
//			get { return false; }
//		}

		/// <summary>
		/// Drops the BackgroundImage property
		/// </summary>
		/// <param name="properties">properties to remove BackGroundImage from</param>
		protected override void PreFilterProperties(System.Collections.IDictionary properties)
		{
			base.PreFilterProperties (properties);
			if (properties.Contains("BackgroundImage") == true)
				properties.Remove("BackgroundImage");
		}

	}
}
