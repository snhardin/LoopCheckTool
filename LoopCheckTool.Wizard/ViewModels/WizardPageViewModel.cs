﻿using LoopCheckTool.Wizard.Models;
using LoopCheckTool.Wizard.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopCheckTool.Wizard.ViewModels
{
    public abstract class WizardPageViewModel : OnPropertyChangedNotifier
    {
        public DocumentGenerationModel Model { get; }

        public WizardPageViewModel(WizardPageViewModel prev)
        {
            Prev = prev;
            Model = prev.Model;
        }

        public WizardPageViewModel()
        {
            Prev = null;
            Model = new DocumentGenerationModel();
        }

        /// <summary>
        /// The next Wizard page to navigate to.
        /// </summary>
        public WizardPageViewModel Next { get; protected set; }

        /// <summary>
        /// The previous Wizard page to navigate back to.
        /// </summary>
        public WizardPageViewModel Prev { get; protected set; }

        /// <summary>
        /// Determines whether or not the user may finish the wizard.
        /// </summary>
        /// <returns>True if Next button can be clicked. False if it cannot.</returns>
        public abstract bool FinishButton_CanExecute();

        /// <summary>
        /// Executed right before the Finish button is clicked.
        /// </summary>
        /// <returns></returns>
        public virtual void FinishButton_BeforeClicked() { }

        /// <summary>
        /// Determines whether or not the user may advance to the
        /// next page of the Wizard on the current step.
        /// </summary>
        /// <returns>True if Next button can be clicked. False if it cannot.</returns>
        public abstract bool NextButton_CanExecute();

        /// <summary>
        /// Executed right before the Next button is clicked.
        /// </summary>
        public virtual void NextButton_BeforeClicked() { }

        /// <summary>
        /// Executed right before the Prev button is clicked.
        /// </summary>
        public virtual void PrevButton_BeforeClicked() { }

        /// <summary>
        /// Executed when the page is navigated to from the Next button.
        /// </summary>
        public virtual void OnNavigateFromNextButton() { }

        /// <summary>
        /// Executed when the page is navigated to from the Prev button.
        /// </summary>
        public virtual void OnNavigateFromPrevButton() { }
    }
}
