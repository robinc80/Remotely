﻿using Remotely.Desktop.Core.ViewModels;
using Remotely.Desktop.Win.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Remotely.Desktop.Win.ViewModels
{
    public class PromptForAccessWindowViewModel : BrandedViewModelBase
    {
        private string _organizationName = "Le garage à PC";
        private string _requesterName = "un technicien";
        public string OrganizationName
        {
            get => _organizationName;
            set
            {
                _organizationName = value;
                FirePropertyChanged();
            }
        }

        public bool PromptResult { get; set; }

        public string RequesterName
        {
            get => _requesterName;
            set
            {
                _requesterName = value;
                FirePropertyChanged();
            }
        }

        public ICommand SetResultNo => new Executor(param =>
        {
            if (param is Window promptWindow)
            {
                PromptResult = false;
                promptWindow.Close();
            }
        });

        public ICommand SetResultYes => new Executor(param =>
                {
            if (param is Window promptWindow)
            {
                PromptResult = true;
                promptWindow.Close();
            }
        });
    }
}
