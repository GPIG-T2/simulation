using Models.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
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

        private const float PressReleaseCost = 0.01f;
        
        private const float BadTestCost = 5.5f;
        private const float GoodTestCost = 140f;

        private const int LowLevelMaskCost = 1;
        private const int HighLevelMaskCost = 15;

        private static int GetTotalPeople(List<string> location)
        {
            return HealthOrganisation.Instance.LocationTrackers[string.Join("", location)].Latest?.GetTotalPeople() ?? -1;
        }

        private static float GetPressReleaseCost(List<string> location)
        {
            var latestInformation = HealthOrganisation.Instance.LocationTrackers[string.Join("", location)].Latest;
            return latestInformation == null ? -1 : latestInformation.GetTotalPeople() * PressReleaseCost;
        }

        public static float CalculateCost(InformationPressRelease pressRelease, ActionMode mode)
        {
            if (mode == ActionMode.Delete)
            {
                return -1;
            }
          
            List<string> location = pressRelease.Location;
            return GetPressReleaseCost(location);
        }

        public static float CalculateCost(TestAndIsolation testAndIsolation, ActionMode mode)
        {
            return mode == ActionMode.Delete ? 0 : testAndIsolation.TestQuality switch
            {
                0 => testAndIsolation.Quantity * BadTestCost,
                1 => testAndIsolation.Quantity * GoodTestCost,
                _ => -1,
            };
        }

        public static float CalculateCost(StayAtHome stayAtHome, ActionMode _)
        {
            return GetPressReleaseCost(stayAtHome.Location);
        }

        public static float CalculateCost(CloseSchools closeSchools, ActionMode _)
        {
            return GetPressReleaseCost(closeSchools.Location);
        }

        public static float CalculateCost(CloseRecreationalLocations closeRecreationalLocations, ActionMode _)
        {
            return GetPressReleaseCost(closeRecreationalLocations.Location);
        }

        public static float CalculateCost(ShieldingProgram shieldingProgram, ActionMode _)
        {
            return GetPressReleaseCost(shieldingProgram.Location);
        }

        public static float CalculateCost(MovementRestrictions movementRestrictions, ActionMode _)
        {
            return GetPressReleaseCost(movementRestrictions.Location);
        }

        public static float CalculateCost(CloseBorders closeBorders, ActionMode _)
        {
            return GetPressReleaseCost(closeBorders.Location);
        }

        public static float CalculateCost(InvestInVaccine investInVaccine, ActionMode mode)
        {
            return mode == ActionMode.Delete ? -1 : investInVaccine.AmountInvested;
        }

        public static float CalculateCost(Furlough furlough, ActionMode _)
        {
            return GetPressReleaseCost(furlough.Location);
        }

        public static float CalculateCost(Loan _, ActionMode mode)
        {
            return mode == ActionMode.Delete ? -1 : 0;
        }

        public static float CalculateCost(MaskMandate maskMandate, ActionMode mode)
        {
            float pressReleaseCost = GetPressReleaseCost(maskMandate.Location);

            if (mode == ActionMode.Delete)
            {
                return pressReleaseCost;
            }

            int population = GetTotalPeople(maskMandate.Location);
            return maskMandate.MaskProvisionLevel switch
            {
                0 => 0,
                1 => pressReleaseCost + LowLevelMaskCost * population,
                2 => pressReleaseCost + HighLevelMaskCost * population,
                _ => -1
            };
        }

        public static float CalculateCost(HealthDrive healthDrive, ActionMode _)
        {
            return GetPressReleaseCost(healthDrive.Location);
        }

        public static float CalculateCost(InvestInHealthServices investInHealthServices, ActionMode mode)
        {
            return mode == ActionMode.Delete ? 0 : investInHealthServices.AmountInvested;
        }

        public static float CalculateCost(SocialDistancingMandate socialDistancingMandate, ActionMode _)
        {
            return GetPressReleaseCost(socialDistancingMandate.Location);
        }

        public static float CalculateCost(Curfew curfew, ActionMode _)
        {
            return GetPressReleaseCost(curfew.Location);
        }

    }
}
