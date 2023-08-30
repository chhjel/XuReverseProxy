export default class StringUtils
{
    static firstOrDefault(val: string | string[]): string | null {
        if (val == null) return null;
        else if (typeof val == 'string') return val;
        else return val[0];
    }
}
