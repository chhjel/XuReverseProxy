export default class ValueUtils {
	public static IsToggleTrue(val: string | boolean): boolean {
		return (
			typeof val === "boolean" && val)
			|| (typeof val === "string" && (val.toLowerCase() == "true" || val == "")
		);
	}
}
