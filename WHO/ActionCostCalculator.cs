using System.Collections.Generic;
using Models.Parameters;
using WHO.Extensions;

namespace WHO
{
    public static class ActionCostCalculator
    {
        public enum ActionMode
        {
            Create,
            Delete
        }

        public const double PressReleaseCost = 0.01d;

        public const double BadTestCost = 5.5d;
        public const double GoodTestCost = 140d;

        public const int LowLevelMaskCost = 1;
        public const int HighLevelMaskCost = 15;

        private static long GetTotalPeople(List<string> location)
        {
            return HealthOrganisation.Instance?.LocationTrackers[location.ToKey()].Latest?.GetTotalPeople() ?? -1;
        }

        private static double GetPressReleaseCost(List<string> location)
        {
            var latestInformation = HealthOrganisation.Instance?.LocationTrackers[location.ToKey()].Latest;
            return latestInformation == null ? -1 : latestInformation.GetTotalPeople() * PressReleaseCost;
        }

        public static double CalculateCost(ParamsContainer action, ActionMode mode)
        {
            double cost = 0;

            switch (action.GetType().Name)
            {
                case nameof(InformationPressRelease):
                    cost = CalculateCost((InformationPressRelease)action, mode);
                    break;
                case nameof(TestAndIsolation):
                    cost = CalculateCost((TestAndIsolation)action, mode);
                    break;
                case nameof(StayAtHome):
                    cost = CalculateCost((StayAtHome)action, mode);
                    break;
                case nameof(CloseSchools):
                    cost = CalculateCost((CloseSchools)action, mode);
                    break;
                case nameof(CloseRecreationalLocations):
                    cost = CalculateCost((CloseRecreationalLocations)action, mode);
                    break;
                case nameof(ShieldingProgram):
                    cost = CalculateCost((ShieldingProgram)action, mode);
                    break;
                case nameof(MovementRestrictions):
                    cost = CalculateCost((MovementRestrictions)action, mode);
                    break;
                case nameof(CloseBorders):
                    cost = CalculateCost((CloseBorders)action, mode);
                    break;
                case nameof(InvestInVaccine):
                    cost = CalculateCost((InvestInVaccine)action, mode);
                    break;
                case nameof(Furlough):
                    cost = CalculateCost((Furlough)action, mode);
                    break;
                case nameof(Loan):
                    cost = CalculateCost((Loan)action, mode);
                    break;
                case nameof(MaskMandate):
                    cost = CalculateCost((MaskMandate)action, mode);
                    break;
                case nameof(HealthDrive):
                    cost = CalculateCost((HealthDrive)action, mode);
                    break;
                case nameof(InvestInHealthServices):
                    cost = CalculateCost((InvestInHealthServices)action, mode);
                    break;
                case nameof(SocialDistancingMandate):
                    cost = CalculateCost((SocialDistancingMandate)action, mode);
                    break;
                case nameof(Curfew):
                    cost = CalculateCost((Curfew)action, mode);
                    break;
            }

            return cost;
        }

        public static double CalculateCost(InformationPressRelease pressRelease, ActionMode mode)
        {
            if (mode == ActionMode.Delete)
            {
                return -1;
            }

            List<string> location = pressRelease.Location;
            return GetPressReleaseCost(location);
        }

        public static double CalculateCost(TestAndIsolation testAndIsolation, ActionMode mode)
        {
            return mode == ActionMode.Delete ? 0 : testAndIsolation.TestQuality switch
            {
                0 => testAndIsolation.Quantity * BadTestCost,
                1 => testAndIsolation.Quantity * GoodTestCost,
                _ => -1,
            };
        }

        public static double CalculateCost(StayAtHome stayAtHome, ActionMode _)
        {
            return GetPressReleaseCost(stayAtHome.Location);
        }

        public static double CalculateCost(CloseSchools closeSchools, ActionMode _)
        {
            return GetPressReleaseCost(closeSchools.Location);
        }

        public static double CalculateCost(CloseRecreationalLocations closeRecreationalLocations, ActionMode _)
        {
            return GetPressReleaseCost(closeRecreationalLocations.Location);
        }

        public static double CalculateCost(ShieldingProgram shieldingProgram, ActionMode _)
        {
            return GetPressReleaseCost(shieldingProgram.Location);
        }

        public static double CalculateCost(MovementRestrictions movementRestrictions, ActionMode _)
        {
            return GetPressReleaseCost(movementRestrictions.Location);
        }

        public static double CalculateCost(CloseBorders closeBorders, ActionMode _)
        {
            return GetPressReleaseCost(closeBorders.Location);
        }

        public static double CalculateCost(InvestInVaccine investInVaccine, ActionMode mode)
        {
            return mode == ActionMode.Delete ? -1 : investInVaccine.AmountInvested;
        }

        public static double CalculateCost(Furlough furlough, ActionMode _)
        {
            return GetPressReleaseCost(furlough.Location);
        }

        public static double CalculateCost(Loan _, ActionMode mode)
        {
            return mode == ActionMode.Delete ? -1 : 0;
        }

        public static double CalculateCost(MaskMandate maskMandate, ActionMode mode)
        {
            double pressReleaseCost = GetPressReleaseCost(maskMandate.Location);

            if (mode == ActionMode.Delete)
            {
                return pressReleaseCost;
            }

            long population = GetTotalPeople(maskMandate.Location);
            return maskMandate.MaskProvisionLevel switch
            {
                0 => 0,
                1 => pressReleaseCost + LowLevelMaskCost * population,
                2 => pressReleaseCost + HighLevelMaskCost * population,
                _ => -1
            };
        }

        public static double CalculateCost(HealthDrive healthDrive, ActionMode _)
        {
            return GetPressReleaseCost(healthDrive.Location);
        }

        public static double CalculateCost(InvestInHealthServices investInHealthServices, ActionMode mode)
        {
            return mode == ActionMode.Delete ? 0 : investInHealthServices.AmountInvested;
        }

        public static double CalculateCost(SocialDistancingMandate socialDistancingMandate, ActionMode _)
        {
            return GetPressReleaseCost(socialDistancingMandate.Location);
        }

        public static double CalculateCost(Curfew curfew, ActionMode _)
        {
            return GetPressReleaseCost(curfew.Location);
        }

    }
}
