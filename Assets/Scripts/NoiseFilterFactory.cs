using UnityEngine;

public static class NoiseFilterFactory
{
   public static INoiseFilter CreateNoiseFilter(NoiseSettings settings)
   {
        switch (settings.filterType)
        {
            case NoiseSettings.FilterType.Simple:
                
        }
   }
}
