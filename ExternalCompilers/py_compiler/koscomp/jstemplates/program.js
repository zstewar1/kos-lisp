{% import 'constants.js' as constants -%}

// Array of all constants.
var ReferencedConstants = [
  {% for constant in ReferencedConstants %}
    %% constants.declare(constant) %%
    %% '' if loop.last else ',' %%
  {% endfor %}
];

// Module loader functions.
var Modules = {
  {% for ident, module in Modules.items() %}
    "%% ident %%": funtion() {
      var import_func = Modules["%% ident %%"];
      var mod = ModuleType.Create(ReferencedConstants[%% module.Identifier %%]);
      // Replace the load function with a function that just returns the newly created
      // instance.
      Modules["%% ident %%"] = function() {
        return mod;
      }

      try {
        {% for op in module.Operations %}
          %% op %%;
        {% endfor %}
      } catch(err) {
        Modules["%% ident %%"] = import_func;
        throw err;
      }

      return mod;
    }
    %% '' if loop.last else ',' %%
  {% endfor %}
}
