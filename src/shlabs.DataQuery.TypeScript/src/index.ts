/** String values accepted by the .NET JsonStringEnumConverter. */
export const QueryOrderDirections = { Asc: "asc", Desc: "desc" } as const;
export type QueryOrderDirections = typeof QueryOrderDirections[keyof typeof QueryOrderDirections];

export const QueryBuilderFilterConditions = { And: "and", Or: "or" } as const;
export type QueryBuilderFilterConditions = typeof QueryBuilderFilterConditions[keyof typeof QueryBuilderFilterConditions];

export const QueryBuilderCriteriaConditions = {
  Equal: "equal",
  NotEqual: "notEqual",
  Greater: "greater",
  Less: "less",
  GreaterOrEqual: "greaterOrEqual",
  LessOrEqual: "lessOrEqual",
  Contains: "contains",
  NotContains: "notContains",
  /** Declared by .NET, but not implemented by its expression converter. */
  In: "in",
  /** Declared by .NET, but not implemented by its expression converter. */
  NotIn: "notIn",
  Null: "null",
  NotNull: "notNull"
} as const;
export type QueryBuilderCriteriaConditions = typeof QueryBuilderCriteriaConditions[keyof typeof QueryBuilderCriteriaConditions];

/** JSON scalar input; the .NET JSON converter stores criteria values as strings. */
export type QueryBuilderFilterValue = string | number | boolean | null;

export interface QueryBuilderFilterCriteria {
  field: string;
  operator: QueryBuilderCriteriaConditions;
  value: QueryBuilderFilterValue;
}

export interface QueryBuilderFilter {
  condition: QueryBuilderFilterConditions;
  rules: QueryBuilderFilterRule[] | null;
}

export type QueryBuilderFilterRule = QueryBuilderFilter | QueryBuilderFilterCriteria;

export interface QueryOrder {
  field: string;
  direction: QueryOrderDirections;
}

/** Wire model for shlabs.DataQuery.Abstractions.Dynamic.QueryBuilder. */
export interface QueryBuilder {
  filter?: QueryBuilderFilter | null;
  order?: QueryOrder[] | null;
  skip?: number | null;
  take?: number | null;
}

export type DynamicQueryBuilder = QueryBuilder;

/** Build a criterion using an entity property path such as "school.name". */
export function criteria(
  field: string,
  operator: QueryBuilderCriteriaConditions,
  value: QueryBuilderFilterValue
): QueryBuilderFilterCriteria {
  return { field, operator, value };
}

export function and(...rules: QueryBuilderFilterRule[]): QueryBuilderFilter {
  return { condition: QueryBuilderFilterConditions.And, rules };
}

export function or(...rules: QueryBuilderFilterRule[]): QueryBuilderFilter {
  return { condition: QueryBuilderFilterConditions.Or, rules };
}

export function asc(field: string): QueryOrder {
  return { field, direction: QueryOrderDirections.Asc };
}

export function desc(field: string): QueryOrder {
  return { field, direction: QueryOrderDirections.Desc };
}

/** Zero-based page number. Supply a deterministic ordering with paged requests. */
export function page(pageNumber: number, pageSize: number): Pick<QueryBuilder, "skip" | "take"> {
  if (!Number.isSafeInteger(pageNumber) || pageNumber < 0) {
    throw new RangeError("pageNumber must be a non-negative safe integer.");
  }
  if (!Number.isSafeInteger(pageSize) || pageSize <= 0) {
    throw new RangeError("pageSize must be a positive safe integer.");
  }
  const skip = pageNumber * pageSize;
  if (skip > 2_147_483_647 || pageSize > 2_147_483_647) {
    throw new RangeError("Paging values must fit a .NET Int32.");
  }
  return { skip, take: pageSize };
}
