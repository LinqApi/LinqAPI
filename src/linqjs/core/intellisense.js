export const DefaultIntellisenseMap = {
  String: ["Contains()", "StartsWith()", "EndsWith()", "Equals"],
  Number: [">", "<", ">=", "<=", "==", "!="],
  Boolean: ["== true", "== false"],
  Date: ["Year", "Month", "Day", "<", ">", "=="]
};

export function getSuggestions(type, context = "String") {
  return DefaultIntellisenseMap[context] || [];
}