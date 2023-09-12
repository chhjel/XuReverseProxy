import { format } from 'date-fns'

export default class DateFormats
{
    public static dateTimeFull(raw: Date | string | null | undefined): string {
        const date = this.parseDate(raw);
        if (date == null) return '';
		return format(date, 'MM/dd/yyyy HH:mm:ss');
	}

    public static defaultDateTime(raw: Date | string | null | undefined): string {
        const date = this.parseDate(raw);
        if (date == null) return '';
		return format(date, 'MM/dd HH:mm');
	}

    public static parseDate(raw: Date | string | null | undefined): Date | null {
		if (raw == null || (typeof raw === 'string' && raw.trim().length == 0)) return null;
		let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
		return date;
	}
}
