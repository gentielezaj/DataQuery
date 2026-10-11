import { and, criteria, asc, page, type DynamicQueryBuilder } from "../src/index.js";

const query: DynamicQueryBuilder = {
  filter: and(criteria("schoolId", "equal", 1)),
  order: [asc("id")],
  ...page(0, 20)
};
void query;

// @ts-expect-error Unsupported operator spelling must be rejected.
criteria("name", "equals", "Jane");
// @ts-expect-error Objects are not scalar criteria values.
criteria("name", "equal", { name: "Jane" });
// @ts-expect-error The root filter must be a group, matching the .NET model.
const invalid: DynamicQueryBuilder = { filter: criteria("id", "equal", 1) };
void invalid;
