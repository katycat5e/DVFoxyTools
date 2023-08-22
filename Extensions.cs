using DV.Localization;
using System.Text;
using UnityEngine;

namespace FoxyTools
{
    public static class Extensions
    {
        public static string Local(this string translationKey, params string[] paramValues)
        {
            return translationKey != null ? LocalizationAPI.L(translationKey, paramValues) : null;
        }

        public static string Heirarchy(this Transform transform)
        {
            return BuildHeirarchy(transform)?.ToString() ?? string.Empty;
        }

        private static StringBuilder BuildHeirarchy(Transform transform)
        {
            if (transform)
            {
                var builder = BuildHeirarchy(transform.parent);

                if (builder == null)
                {
                    builder = new StringBuilder(transform.name);
                }
                else
                {
                    builder.Append('/');
                    builder.Append(transform.name);
                }

                return builder;
            }

            return null;
        }
    }
}
