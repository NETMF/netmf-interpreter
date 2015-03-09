using System;
using System.Windows.Forms.Design;

namespace Gui.Wizard
{
	/// <summary>
	/// 
	/// </summary>
	public class HeaderDesigner : ParentControlDesigner
	{

		/// <summary>
		/// Prevents the grid from being drawn on the Wizard
		/// </summary>
		protected override bool DrawGrid
		{
			get { return false; }
		}

		/// <summary>
		/// Drops the BackgroundImage property
		/// </summary>
		/// <param name="properties">properties to remove BackGroundImage from</param>
		protected override void PreFilterProperties(System.Collections.IDictionary properties)
		{
			base.PreFilterProperties (properties);
			if (properties.Contains("BackgroundImage") == true)
				properties.Remove("BackgroundImage");
			if (properties.Contains("DrawGrid") == true)
				properties.Remove("DrawGrid");
		}

	}
}
