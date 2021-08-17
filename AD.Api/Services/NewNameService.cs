using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using AD.Api.Components;
using AD.Api.Extensions;
using AD.Api.Models;
using Microsoft.Extensions.Options;
using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Services
{
    public interface INewNameService
    {
        string Construct(JsonCreateUser user);
    }

    [SupportedOSPlatform("windows")]
    public class NewNameService : INewNameService
    {
        private string _format;
        private int _count;

        private List<Func<JsonCreateUser, string>> _variableList;
        private static readonly Dictionary<string, Func<JsonCreateUser, string>> _variables =
            new Dictionary<string, Func<JsonCreateUser, string>>(5, StringComparer.CurrentCultureIgnoreCase)
            {
                { "{cn}", x => x.CommonName },
                { "{commonName}", x => x.CommonName },
                { "{name}", x => x.Name },
                { "{first}", x => x.GivenName },
                { "{givenName}", x => x.GivenName },
                //{ "{initial}", x => x }
                { "{last}", x => x.Surname },
                { "{surname}", x => x.Surname },
                { "{sn}", x => x.Surname }
            };

        public NewNameService(IOptions<CreationOptions> options)
        {
            _variableList = new List<Func<JsonCreateUser, string>>(2);
            if (string.IsNullOrWhiteSpace(options.Value.NewNameFormat))
                _variableList.Add(x => x.Name);

            else
                CreateConstructor(options.Value.NewNameFormat, ref _variableList);

            _format = Regex.Replace(
                options.Value.NewNameFormat,
                Strings.NameCtor_RegexReplace,
                new MatchEvaluator(this.Replace)
            );
        }

        public string Construct(JsonCreateUser user)
        {
            return _format.Format(this.GetValues(user));
        }

        private string[] GetValues(JsonCreateUser user)
        {
            string[] strArray = new string[_variableList.Count];
            for (int i = 0; i < _variableList.Count; i++)
            {
                strArray[i] = _variableList[i](user);
            }

            return strArray;
        }
        private string Replace(Match match)
        {
            string output = "{{{0}}}".Format(_count);
            _count++;
            return output;
        }

        private static void CreateConstructor(string format, ref List<Func<JsonCreateUser, string>> list)
        {
            MatchCollection matchCol = Regex.Matches(format, Strings.NameCtor_Regex);
            if (null != matchCol && matchCol.Count > 0)
            {
                for (int i = 0; i < matchCol.Count; i++)
                {
                    Match m = matchCol[i];
                    if (m.Success &&
                        m.Groups.Count >= 2 &&
                        !string.IsNullOrEmpty(m.Groups[1].Value) &&
                        _variables.TryGetValue(m.Groups[1].Value, out Func<JsonCreateUser, string> function))
                    {
                        list.Add(function);
                    }
                }
            }

            if (list.Count <= 0)
                list.Add(x => x.Name);
        }
    }
}
