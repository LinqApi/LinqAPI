// core/models.js

export class Query {
  constructor(controller = "", {
    filter = new LogicalFilter("AND"),
    pager = new Pager(),
    orderBy = "id",
    desc = true,
    groupBy = "",
    select = "",
    includes = [],
  } = {}) {
    this.controller = controller;
    this.filter = filter;
    this.pager = pager;
    this.orderBy = orderBy;
    this.desc = desc;
    this.groupBy = groupBy;
    this.select = select;
    this.includes = includes;
  }

  toPayload() {
    return {
      filter: this.filter.toString(),
      pager: this.pager,
      orderBy: this.orderBy,
      desc: this.desc,
      groupBy: this.groupBy,
      select: this.select,
      includes: this.includes.map(i => i.toPayload()),
    };
  }
}

export class Include {
  constructor(propertyName, pager = new Pager(), thenIncludes = []) {
    this.propertyName = propertyName;
    this.pager = pager;
    this.thenIncludes = thenIncludes;
  }

  toPayload() {
    return {
      propertyName: this.propertyName,
      pager: this.pager,
      thenIncludes: this.thenIncludes.map(ti => ti.toPayload()),
    };
  }
}

export class ThenInclude {
  constructor(parentProperty, childIncludes = [], pager = new Pager()) {
    this.parentProperty = parentProperty;
    this.childIncludes = childIncludes;
    this.pager = pager;
  }

  toPayload() {
    return {
      parentProperty: this.parentProperty,
      childIncludes: this.childIncludes,
      pager: this.pager,
    };
  }
}

export class Pager {
  constructor(pageNumber = 1, pageSize = 10) {
    this.pageNumber = pageNumber;
    this.pageSize = pageSize;
  }
}

export class Filter {
  toString() { return ""; }
}

export class LogicalFilter extends Filter {
  constructor(operator = "AND", filters = []) {
    super();
    this.operator = operator;
    this.filters = filters;
  }

  toString() {
    if (!this.filters.length) return "1=1";
    return `(${this.filters.map(f => f.toString()).join(` ${this.operator} `)})`;
  }
}

export class ComparisonFilter extends Filter {
  constructor(field, operator, value) {
    super();
    this.field = field;
    this.operator = operator;
    this.value = value;
  }

  toString() {
    return `${this.field} ${this.operator} ${this.value}`;
  }
}
