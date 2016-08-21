﻿using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Localization;
using Cinotam.AbpModuleZero;
using Cinotam.AbpModuleZero.Tools.DatatablesJsModels.GenericTypes;
using Cinotam.ModuleZero.AppModule.Languages.Dto;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Cinotam.ModuleZero.AppModule.Languages
{
    public class LanguageAppService : CinotamModuleZeroAppServiceBase, ILanguageAppService
    {
        private readonly IApplicationLanguageManager _applicationLanguageManager;
        private readonly IApplicationLanguageTextManager _applicationLanguageTextManager;
        private readonly IRepository<ApplicationLanguageText, long> _languageTextsRepository;
        private readonly IRepository<ApplicationLanguage> _languagesRepository;
        public LanguageAppService(IApplicationLanguageManager applicationLanguageManager, IApplicationLanguageTextManager applicationLanguageTextManager, IRepository<ApplicationLanguageText, long> languageTextsRepository, IRepository<ApplicationLanguage> languagesRepository)
        {
            _applicationLanguageManager = applicationLanguageManager;
            _applicationLanguageTextManager = applicationLanguageTextManager;
            _languageTextsRepository = languageTextsRepository;
            _languagesRepository = languagesRepository;
        }
        /// <summary>
        /// Adds a new available language to the app
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task AddLanguage(LanguageInput input)
        {
            await _applicationLanguageManager.AddAsync(new ApplicationLanguage(AbpSession.TenantId, input.LangCode, input.DisplayName, input.Icon));
        }

        public ReturnModel<LanguageDto> GetLanguagesForTable(RequestModel<object> input)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                int totalCount;

                var allLanguages = _languagesRepository.GetAll();
                var filterByLength = GenerateTableModel(input, allLanguages, "Name", out totalCount).ToList();

                return new ReturnModel<LanguageDto>()
                {
                    draw = input.draw,
                    iTotalDisplayRecords = totalCount,
                    recordsTotal = totalCount,
                    iTotalRecords = allLanguages.Count(),
                    recordsFiltered = filterByLength.Count,
                    length = input.length,
                    data = filterByLength.Select(a => new LanguageDto()
                    {
                        Icon = a.Icon,
                        DisplayName = a.DisplayName,
                        Name = a.Name,
                        Id = a.Id,
                        CreationTime = a.CreationTime
                    }).ToArray()
                };
            }

        }

        public async Task AddEditLocalizationText(LocalizationTextInput input)
        {
            await _applicationLanguageTextManager.UpdateStringAsync(
                AbpSession.TenantId,
                AbpModuleZeroConsts.LocalizationSourceName,
                CultureInfo.GetCultureInfo(input.Culture),
                input.Key, input.Value);
        }
        /// <summary>
        /// Experimental (Todo:Find the way to make just one ajax call to this function per operation)
        /// Note: if there is a lot of language text elements in the table this may cause some perf. issues
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public ReturnModel<LanguageTextTableElement> GetLocalizationTexts(RequestModel<LanguageTextsForEditRequest> input)
        {
            //In memory watch out!
            var languageTextsSource = _languageTextsRepository.GetAll()
                .Where(a => a.Source == input.TypeOfRequest.Source
                && a.LanguageName == input.TypeOfRequest.SourceLang).ToList();
            var languageTextsTarget = _languageTextsRepository.GetAll()
                .Where(a => a.Source == input.TypeOfRequest.Source
                && a.LanguageName == input.TypeOfRequest.TargetLang).ToList();
            //
            //Ahora a comparar

            var listOfElements = new List<LanguageTextTableElement>();
            foreach (var applicationLanguageText in languageTextsSource)
            {
                listOfElements.Add(new LanguageTextTableElement()
                {
                    Key = applicationLanguageText.Key,
                    Source = applicationLanguageText.Source,
                    SourceValue = applicationLanguageText.Value,
                    TargetValue = GetTargetValueFromList(languageTextsTarget, applicationLanguageText.Key)
                });
            }


            return new ReturnModel<LanguageTextTableElement>()
            {
                data = listOfElements.ToArray()
            };
        }

        private string GetTargetValueFromList(List<ApplicationLanguageText> languageTextsTarget, string key)
        {
            if (languageTextsTarget.All(a => a.Key != key))
            {
                return "";
            }
            var first = languageTextsTarget.FirstOrDefault(a => a.Key == key);
            return string.IsNullOrEmpty(first?.Value) ? "" : first.Value;
        }

        public LanguageTextsForEditView GetLanguageTextsForEditView(string selectedTargetLanguage,
            string selectedSourceLanguage)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var sources = _languageTextsRepository.GetAll();
                var result = (from s in sources group s by s.Source into grp select grp.Key).ToList();
                var languages = _languagesRepository.GetAll().ToList();
                return new LanguageTextsForEditView()
                {
                    SourceLanguages = languages.Select(a => new LanguageSelected(a.DisplayName, a.Name, a.Icon)).ToList(),
                    TargetLanguages = languages.Select(a => new LanguageSelected(a.DisplayName, a.Name, a.Icon)).ToList(),
                    SelectedSourceLanguage = selectedSourceLanguage,
                    SelectedTargetLanguage = selectedTargetLanguage,
                    Source = result.Any() ?
                    result.Select(a => a).ToList() :
                    new List<string>()
                    {
                        AbpModuleZeroConsts.LocalizationSourceName
                    }
                };
            }
        }
        #region UnusedCode

        /*For GetLocalizationTexts
         
            Main Idea:

                source = String
                sources = List<LanguageText.cs>
                targets = List<LanguageText.cs>
            
            Params: 
            
                object <LanguageTextsForEditRequest.cs>

            Returns:

                LanguageTextsForEdit.cs

            Why Unused:

                -This will require one additional list
                -Its not suitable for table elements
                -Too hard to implement with datatables.js

            //////////////////////////////

            var languageTextsSource = _languageTextsRepository.GetAll().Where(a => a.Source == input.TypeOfRequest.Source && a.LanguageName == input.TypeOfRequest.SourceLang);
            
            var languageTextsTarget = _languageTextsRepository.GetAll().Where(a => a.Source == input.TypeOfRequest.Source && a.LanguageName == input.TypeOfRequest.TargetLang);

            var targetLanguageTexts = new List<LanguageText>();

            var sourceLanguageTexts = new List<LanguageText>();
            
            foreach (var applicationLanguageTextSrc in languageTextsSource)
            {

                //Adds source 
                sourceLanguageTexts.Add(new LanguageText()
                {
                    Key = applicationLanguageTextSrc.Key,
                    Value = applicationLanguageTextSrc.Value
                });



                //Buscar coincidencia en target
                var element = languageTextsTarget.FirstOrDefault(a => a.Key == applicationLanguageTextSrc.Key);
                if (element == null)
                {
                    //Adds an empty field
                    targetLanguageTexts.Add(new LanguageText()
                    {
                        Key = applicationLanguageTextSrc.Key,
                        Value = ""
                    });
                }
                else
                {
                    //Found adds the current value
                    targetLanguageTexts.Add(new LanguageText()
                    {
                        Key = element.Key,
                        Value = element.Value
                    });
                }
            }
            return new LanguageTextsForEdit()
            {
                Source = input.TypeOfRequest.Source,
                SourceLanguageTexts = sourceLanguageTexts,
                TargetLanguageTexts = targetLanguageTexts
            };

            //////////////////////////////

         */


        #endregion
    }
}