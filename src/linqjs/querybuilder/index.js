export function buildDynamicQuery(filter, groupBy, orderBy, select) {
    let queryParts = [];
    
    if (filter) queryParts.push(`filter: ${filter}`);
    if (groupBy) queryParts.push(`groupBy: ${groupBy}`);
    if (orderBy) queryParts.push(`orderBy: ${orderBy}`);
    if (select) queryParts.push(`select: ${select}`);

    return `{ ${queryParts.join(", ")} }`;
}
