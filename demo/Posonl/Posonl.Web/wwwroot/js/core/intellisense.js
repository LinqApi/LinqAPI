// File: /js/core/IntelliSense.js

//export const IntelliSenseMappings = {
//    filter: {
//        string: [
//            { name: "Contains()", tooltip: 'Usage: Name.Contains("Stripe")' },
//            { name: "StartsWith()", tooltip: 'Usage: Name.StartsWith("St")' },
//            { name: "EndsWith()", tooltip: 'Usage: Name.EndsWith("pe")' },
//            { name: "Equals", tooltip: 'Usage: Name == "Stripe"' },
//            { name: "Length", tooltip: 'Usage: Name.Length > 5' }
//        ],
//        boolean: [
//            { name: "== true", tooltip: 'Usage: IsActive == true' },
//            { name: "== false", tooltip: 'Usage: IsActive == false' }
//        ],
//        int32: [
//            { name: ">", tooltip: "Usage: Count > 10" },
//            { name: "<", tooltip: "Usage: Count < 100" },
//            { name: "==", tooltip: "Usage: Count == 5" },
//            { name: ">=", tooltip: "Usage: Count >= 1" },
//            { name: "<=", tooltip: "Usage: Count <= 10" }
//        ],
//        int64: [
//            { name: ">", tooltip: "Usage: Amount > 1000" },
//            { name: "<", tooltip: "Usage: Amount < 5000" },
//            { name: "==", tooltip: "Usage: Amount == 2000" },
//            { name: ">=", tooltip: "Usage: Amount >= 2000" },
//            { name: "<=", tooltip: "Usage: Amount <= 3000" }
//        ],
//        dateTime: [
//            { name: ">", tooltip: 'Usage: CreatedDate > "2023-01-01"' },
//            { name: "<", tooltip: 'Usage: CreatedDate < "2024-01-01"' },
//            { name: "==", tooltip: 'Usage: CreatedDate == "2024-03-01"' },
//            { name: ".Year", tooltip: 'Usage: CreatedDate.Year == 2024' },
//            { name: ".Month", tooltip: 'Usage: CreatedDate.Month == 3' },
//            { name: ".Day", tooltip: 'Usage: CreatedDate.Day == 15' },
//            { name: ".DayOfWeek", tooltip: 'Usage: CreatedDate.DayOfWeek == 1' }
//        ],
//        "dateTime?": [
//            { name: ".HasValue", tooltip: 'Usage: CreatedDate.HasValue == true' },
//            { name: ".Value", tooltip: 'Usage: CreatedDate.Value.Year == 2023' },
//            { name: ".Year", tooltip: 'Usage: CreatedDate.Value.Year == 2024' },
//            { name: ".Month", tooltip: 'Usage: CreatedDate.Value.Month == 3' },
//            { name: ".Day", tooltip: 'Usage: CreatedDate.Value.Day == 1' }
//        ]
//    },
//    groupBy: {
//        dateTime: [
//            { name: ".Year", tooltip: "Group by year" },
//            { name: ".Month", tooltip: "Group by month" },
//            { name: ".Day", tooltip: "Group by day" },
//            { name: ".DayOfWeek", tooltip: "Group by day of week" }
//        ],
//        string: [
//            { name: ".Length", tooltip: "Group by length" }
//        ],
//        int32: [],
//        int64: []
//    },
//    // Dynamically build select suggestions if a groupBy field is provided.
//    buildSelectSuggestions: (groupByField) => {
//        if (!groupByField) return [];
//        // Remove trailing accessor parts such as .Value, .Year, etc.
//        const baseField = groupByField.replace(/\.(Value|Year|Month|Day)$/i, "").trim();
//        const core = `Key as ${baseField}`;
//        const numericFns = ["Count", "Sum", "Avg", "Max", "Min"];
//        return numericFns.map(fn => ({
//            name: `new(${core}, ${fn}() as ${fn.toLowerCase()})`,
//            tooltip: `Aggregate ${baseField} with ${fn}`
//        }));
//    },
//    orderBy: {
//        descTooltip: "Descending order if checked, ascending otherwise"
//    }
//};

/**
 * Retrieves IntelliSense suggestions for a given context and property type.
 * @param {string} context - e.g., "filter", "groupby", "select", "include"
 * @param {string} propertyType - The type of the property (e.g., "string", "int32", "dateTime")
 * @param {string} currentInput - The current text in the input field
 * @param {object} [extra] - Additional data (e.g., for select suggestions: { groupByField })
 * @returns {Array} Array of suggestion objects
 */
//export function getIntelliSuggestions(context, propertyType, currentInput = "", extra = {}) {
//    // Normalize the propertyType to lower case for mapping lookup.
//    let typeKey = propertyType.toLowerCase();

//    if (context === "filter") {
//        const suggestions = IntelliSenseMappings.filter[typeKey] || [];
//        // If input is empty, return all suggestions; otherwise, filter by current input.
//        if (!currentInput.trim()) return suggestions;
//        return suggestions.filter(s => s.name.toLowerCase().includes(currentInput.toLowerCase()));
//    }
//    if (context === "groupby") {
//        return IntelliSenseMappings.groupBy[typeKey] || [];
//    }
//    if (context === "select") {
//        return IntelliSenseMappings.buildSelectSuggestions(extra.groupByField);
//    }
//    if (context === "include") {
//        // For include context, you might return an empty array or a dedicated list.
//        return [];
//    }
//    return [];
//}
