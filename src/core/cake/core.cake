#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
#addin "nuget:?package=Cake.Json&version=3.0.1"
#addin "nuget:?package=Newtonsoft.Json&version=11.0.2"

#load "./template.cake"

var packerTemplates = new List<PackerTemplate>();

IEnumerable<PackerTemplate> PackerTemplates_CreateWindows(string name, string groupName = null, string groupVersion = null, IEnumerable<PackerTemplate> parents = null) {
  var items = new List<PackerTemplate>();

  if (parents == null) {
    var virtualBoxInit = PackerTemplate_Create(
      name,
      "virtualbox-init",
      new [] { PackerBuilder_Create("virtualbox-iso") },
      new [] { PackerProvisioner_Create("vagrant") },
      new [] { PackerPostProcessor_Create("vagrant-virtualbox") },
      null
    );
    items.Add(virtualBoxInit);
  }
  var virtualBoxCore = PackerTemplate_Create(
    name,
    "virtualbox-core",
    new [] { PackerBuilder_Create(parents == null ? "virtualbox-iso" : "virtualbox-ovf") },
    new [] { PackerProvisioner_Create("chef") },
    new [] { PackerPostProcessor_Create("manifest") },
    parents != null ? parents.First(item => item.IsMatching("virtualbox-core")) : null
  );
  var virtualBoxVagrant = PackerTemplate_Create(
    name,
    "virtualbox-vagrant",
    new [] { PackerBuilder_Create("virtualbox-ovf") },
    new [] { PackerProvisioner_Create("vagrant") },
    new [] { PackerPostProcessor_Create("vagrant-virtualbox") },
    virtualBoxCore,
    groupName,
    groupVersion
  );
  items.Add(virtualBoxCore);
  items.Add(virtualBoxVagrant);

  if (parents == null) {
    var hyperVInit = PackerTemplate_Create(
      name,
      "hyperv-init",
      new [] { PackerBuilder_Create("hyperv-iso") },
      new [] { PackerProvisioner_Create("vagrant") },
      new [] { PackerPostProcessor_Create("vagrant-hyperv") },
      null
    );
    items.Add(hyperVInit);
  }
  var hyperVCore = PackerTemplate_Create(
    name,
    "hyperv-core",
    new [] { PackerBuilder_Create(parents == null ? "hyperv-iso" : "hyperv-vmcx") },
    new [] { PackerProvisioner_Create("chef") },
    new [] { PackerPostProcessor_Create("manifest") },
    parents != null ? parents.First(item => item.IsMatching("hyperv-core")) : null
  );
  var hyperVVagrant = PackerTemplate_Create(
    name,
    "hyperv-vagrant",
    new [] { PackerBuilder_Create("hyperv-vmcx") },
    new [] { PackerProvisioner_Create("vagrant") },
    new [] { PackerPostProcessor_Create("vagrant-hyperv") },
    hyperVCore,
    groupName,
    groupVersion
  );
  items.Add(hyperVCore);
  items.Add(hyperVVagrant);

  packerTemplates.AddRange(items);

  return items;
}

IEnumerable<PackerTemplate> PackerTemplates_CreateLinux(string name, string groupName = null, string groupVersion = null, IEnumerable<PackerTemplate> parents = null, bool amazon = true, bool azure = true) {
  var items = new List<PackerTemplate>();

  if (parents == null) {
    var virtualBoxInit = PackerTemplate_Create(
      name,
      "virtualbox-init",
      new [] { PackerBuilder_Create("virtualbox-iso") },
      new [] { PackerProvisioner_Create("shell-install"), PackerProvisioner_Create("shell-vagrant") },
      new [] { PackerPostProcessor_Create("vagrant-virtualbox") },
      null
    );
    items.Add(virtualBoxInit);
  }
  var virtualBoxCore = PackerTemplate_Create(
    name,
    "virtualbox-core",
    new [] { PackerBuilder_Create(parents == null ? "virtualbox-iso" : "virtualbox-ovf") },
    new [] { PackerProvisioner_Create("shell-prepare"), PackerProvisioner_Create("shell-configure"), PackerProvisioner_Create("shell-install"), PackerProvisioner_Create("shell-cleanup") },
    new [] { PackerPostProcessor_Create("manifest") },
    parents != null ? parents.First(item => item.IsMatching("virtualbox-core")) : null
  );
  var virtualBoxVagrant = PackerTemplate_Create(
    name,
    "virtualbox-vagrant",
    new [] { PackerBuilder_Create("virtualbox-ovf") },
    new [] { PackerProvisioner_Create("shell-vagrant") },
    new [] { PackerPostProcessor_Create("vagrant-virtualbox") },
    virtualBoxCore,
    groupName,
    groupVersion
  );
  items.Add(virtualBoxCore);
  items.Add(virtualBoxVagrant);

  if (parents == null) {
    var hyperVInit = PackerTemplate_Create(
      name,
      "hyperv-init",
      new [] { PackerBuilder_Create("hyperv-iso") },
      new [] { PackerProvisioner_Create("shell-vagrant") },
      new [] { PackerPostProcessor_Create("vagrant-hyperv") },
      null
    );
    items.Add(hyperVInit);
  }
  var hyperVCore = PackerTemplate_Create(
    name,
    "hyperv-core",
    new [] { PackerBuilder_Create(parents == null ? "hyperv-iso" : "hyperv-vmcx") },
    new [] { PackerProvisioner_Create("shell-prepare"), PackerProvisioner_Create("shell-configure"), PackerProvisioner_Create("shell-install"), PackerProvisioner_Create("shell-cleanup") },
    new [] { PackerPostProcessor_Create("manifest") },
    parents != null ? parents.First(item => item.IsMatching("hyperv-core")) : null
  );
  var hyperVVagrant = PackerTemplate_Create(
    name,
    "hyperv-vagrant",
    new [] { PackerBuilder_Create("hyperv-vmcx") },
    new [] { PackerProvisioner_Create("shell-vagrant") },
    new [] { PackerPostProcessor_Create("vagrant-hyperv") },
    hyperVCore,
    groupName,
    groupVersion
  );
  items.Add(hyperVCore);
  items.Add(hyperVVagrant);

  if (amazon) {
    var amazonCore = PackerTemplate_Create(
      name,
      "amazon",
      new [] { PackerBuilder_Create("amazon") },
      new [] { PackerProvisioner_Create("shell-prepare"), PackerProvisioner_Create("shell-configure"), PackerProvisioner_Create("shell-install"), PackerProvisioner_Create("shell-cleanup") },
      new [] { PackerPostProcessor_Create("manifest") },
      // new [] { PackerPostProcessor_Create("vagrant-amazon") },
      parents != null ? parents.First(item => item.IsMatching("amazon")) : null
    );
    items.Add(amazonCore);
  }

  if (azure) {
    var azureCore = PackerTemplate_Create(
      name,
      "azure",
      new [] { PackerBuilder_Create(parents == null ? "azure-marketplace" : "azure-custom") },
      new [] { PackerProvisioner_Create("shell-prepare"), PackerProvisioner_Create("shell-configure"), PackerProvisioner_Create("shell-install"), PackerProvisioner_Create("shell-cleanup") },
      new [] { PackerPostProcessor_Create("manifest") },
      parents != null ? parents.First(item => item.IsMatching("azure")) : null
    );

    items.Add(azureCore);
  }

  packerTemplates.AddRange(items);

  return items;
}

void PackerTemplates_ForEach(string template, Action<PackerTemplate> action) {
  foreach (var t in packerTemplates.Where(item => item.IsMatching(template))) {
    action(t);
  }
}
