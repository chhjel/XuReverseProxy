
export function htmlEncode(input: string): string {
  if (!input) return input;
  else return input.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
}

export function htmlAttributeEncode(input: string): string {
  if (!input) return input;
  else return input.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&#34;").replace(/'/g, "&#39;");
}
