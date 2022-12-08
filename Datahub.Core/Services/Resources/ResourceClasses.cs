using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services.Resources
{
    public abstract class AbstractResource
    {
        public string Title { get; private set; }
        public AbstractResource Parent { get; private set; }
        protected AbstractResource(string title, AbstractResource parent)
        {
            Title = title;
            Parent = parent;
        }
    }

    public class ResourceCard : AbstractResource
    {
        public string Preview { get; private set; }
        public string Url { get; private set; }
        public ResourceCategory ParentCategory => Parent as ResourceCategory;

        public ResourceCard(string title, string preview, string url, ResourceCategory category): base(title, category)
        {
            Preview = preview;
            Url = url;
            category.AddCard(this);
        }
    }

    public class ResourceCategory : AbstractResource
    {
        private readonly List<ResourceCard> _cards;

        public IEnumerable<ResourceCard> Cards => _cards.AsReadOnly();
        private ResourceLanguageRoot ParentLanguage => Parent as ResourceLanguageRoot;

        public ResourceCategory(string title, ResourceLanguageRoot languageRoot) : base(title, languageRoot)
        {
            _cards = new();
            languageRoot.AddCategory(this);
        }

        public void AddCard(ResourceCard card)
        {
            _cards.Add(card);
            ParentLanguage?.Index(card);
        }
    }

    public class ResourceLanguageRoot : AbstractResource
    {
        private Dictionary<string, AbstractResource> _resourceIndex;
        private readonly List<ResourceCategory> _categories;

        public IDictionary<string, AbstractResource> ResourceIndex => _resourceIndex.AsReadOnly();
        public IEnumerable<ResourceCategory> Categories => _categories.AsReadOnly();
        public IEnumerable<ResourceCard> AllCards => Categories.SelectMany(c => c.Cards);

        public AbstractResource this[string key] => _resourceIndex.TryGetValue(key, out AbstractResource resource) ? resource : null;

        public ResourceLanguageRoot(string Language) : base(Language, null)
        {
            _resourceIndex = new Dictionary<string, AbstractResource>();
            _categories = new List<ResourceCategory>();
        }

        public void RefreshIndex()
        {
            var catIndex = _categories.ToDictionary(c => c.Title, c => c as AbstractResource);
            var itemIndex = _categories.SelectMany(c => c.Cards).ToDictionary(c => c.Title, c => c as AbstractResource);
            _resourceIndex = catIndex.Union(itemIndex).ToDictionary(c => c.Key, c => c.Value);
        }

        public void Index(AbstractResource resource)
        {
            if (resource == null) return;

            _resourceIndex[resource.Title] = resource;
        }

        public void AddCategory(ResourceCategory category)
        {
            _categories.Add(category);
            Index(category);
        }
    }
}