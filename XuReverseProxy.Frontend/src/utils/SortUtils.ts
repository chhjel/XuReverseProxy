
export function SortBy<TItem, TPropA>(
    a:TItem, b:TItem,
    firstPropSelector: (item:TItem) => TPropA,
    firstComparator: null | ((a:TItem, b:TItem) => number) = null,
    invertFirst: boolean = false
): number {
    return SortByThenBy(a, b, firstPropSelector, null, firstComparator, null, invertFirst, false);
}

export function SortByThenBy<TItem, TPropA, TPropB>(
    a:TItem, b:TItem,
    firstPropSelector: (item:TItem) => TPropA,
    secondPropSelector: ((item:TItem) => TPropB) | null,
    firstComparator: null | ((a:TItem, b:TItem) => number) = null,
    secondComparator: null | ((a:TItem, b:TItem) => number) = null,
    invertFirst: boolean = false,
    invertSecond: boolean = false
): number {
    // Order by..
    if (firstComparator != null) {
        const val = firstComparator(a, b);
        if (val != 0) return invertFirst ? -val : val;
    } else {
        if (firstPropSelector(a) > firstPropSelector(b)) {
            return invertFirst ? 1 : -1;
        } else if (firstPropSelector(a) < firstPropSelector(b)) { 
            return invertFirst ? -1 : 1;
        }
    }

    if (secondPropSelector === null) {
        return 0;
    }

    // Then by..
    if (secondComparator != null) {
        const val = secondComparator(a, b);
        if (val != 0) return invertSecond ? -val : val;
    } else {
        if (secondPropSelector(a) > secondPropSelector(b)) {
            return invertSecond ? 1 : -1;
        } else if (secondPropSelector(a) < secondPropSelector(b)) { 
            return invertSecond ? -1 : 1;
        } else {
            return 0;
        }
    }
}
