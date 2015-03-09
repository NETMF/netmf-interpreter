using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.Diagnostics;

namespace Gui.Wizard
{
	/// <summary>
	/// 
	/// </summary>
	[Designer(typeof(Gui.Wizard.WizardPageDesigner))]
	public class WizardPage : Panel
	{

		/// <summary>
		/// Event called before this page is closed when the back button is pressed. If you don't want to show pageIndex -1 then
		/// set page to be the new page that you wish to show
		/// </summary>
		public event PageEventHandler CloseFromBack;
		/// <summary>
		/// Event called before this page is closed when the next button is pressed. If you don't want to show pageIndex -1 then
		/// set page to be the new page that you wish to show 
		/// </summary>
		public event PageEventHandler CloseFromNext;
		/// <summary>
		/// Event called after this page is shown when the back button is pressed.
		/// </summary>
		public event EventHandler ShowFromBack;
		/// <summary>
		/// Event called after this page is shown when the next button is pressed. 
		/// </summary>
		public event EventHandler ShowFromNext;

		/// <summary>
		/// Fires the CloseFromBack Event
		/// </summary>
		/// <param name="wiz">Wizard to pass into the sender argument</param>
		/// <returns>Index of Page that the event handlers would like to see next</returns>
		public int OnCloseFromBack(Wizard wiz)
		{
			//Event args thinks that the next pgae will be the one before this one
			PageEventArgs e = new PageEventArgs(wiz.PageIndex -1, wiz.Pages);
			//Tell anybody who listens
			if (CloseFromBack != null)
				CloseFromBack(wiz, e);
			//And then tell whomever called me what the prefered page is
			return e.PageIndex;
		}

		/// <summary>
		/// Fires the CloseFromNextEvent
		/// </summary>
		/// <param name="wiz">Sender</param>
		public int OnCloseFromNext(Wizard wiz)
		{	
			//Event args thinks that the next pgae will be the one before this one
			PageEventArgs e = new PageEventArgs(wiz.PageIndex +1, wiz.Pages);
			//Tell anybody who listens
			if (CloseFromNext != null)
				CloseFromNext(wiz, e);
			//And then tell whomever called me what the prefered page is
			return e.PageIndex;
		}
		
		/// <summary>
		/// Fires the showFromBack event
		/// </summary>
		/// <param name="wiz">sender</param>
		public void OnShowFromBack(Wizard wiz)
		{
			if (ShowFromBack != null)
				ShowFromBack(wiz, EventArgs.Empty);
		}

		/// <summary>
		/// Fires the showFromNext event
		/// </summary>
		/// <param name="wiz">Sender</param>
		public void OnShowFromNext(Wizard wiz)
		{
			if (ShowFromNext != null)
				ShowFromNext(wiz, EventArgs.Empty);
		}


		[Category("Wizard")]
		public bool IsFinishPage
		{
			get
			{
				return _IsFinishPage;
			}
			set
			{
				_IsFinishPage=value;
			}
		}
		private bool _IsFinishPage = false;
	
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing"></param>

		protected override void Dispose( bool disposing ) 
		{
			if( disposing ) 
			{
//				//Unregister callbacks
//				ClearChangeNotifications();      
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set the focus to the contained control with a lowest tabIndex
		/// </summary>
		public void FocusFirstTabIndex()
		{
			//Activate the first control in the Panel
			Control found = null;
			//find the control with the lowest 
			foreach (Control control in this.Controls)
			{
				if (control.CanFocus && (found == null || control.TabIndex < found.TabIndex))
				{
					found = control;
				}
			}
			//Have we actually found anything
			if (found != null)
			{
				//Focus the found control
				found.Focus();
			}
			else
			{
				//Just focus the wizard Page
				this.Focus();
			}
		}

	}
}