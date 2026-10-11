import assert from "node:assert/strict";
import test from "node:test";
import { and, or, criteria, asc, desc, page, QueryBuilderCriteriaConditions as Operators } from "../dist/index.js";

test("helpers serialize nested filters, scalar values, ordered keys, and paging for the API", () => {
  const query = {
    filter: and(
      criteria("schoolId", Operators.Equal, 1),
      or(criteria("name", Operators.Contains, "Jane"), criteria("isActive", Operators.Equal, true))
    ),
    order: [asc("school.name"), desc("id")],
    ...page(2, 20)
  };
  assert.deepEqual(JSON.parse(JSON.stringify(query)), {
    filter: {
      condition: "and",
      rules: [
        { field: "schoolId", operator: "equal", value: 1 },
        { condition: "or", rules: [
          { field: "name", operator: "contains", value: "Jane" },
          { field: "isActive", operator: "equal", value: true }
        ] }
      ]
    },
    order: [{ field: "school.name", direction: "asc" }, { field: "id", direction: "desc" }],
    skip: 40,
    take: 20
  });
});

test("empty filter groups and null criteria values retain their JSON representation", () => {
  assert.deepEqual(and(), { condition: "and", rules: [] });
  assert.deepEqual(criteria("name", Operators.Null, null), { field: "name", operator: "null", value: null });
});

test("paging rejects invalid numbers and values exceeding the .NET wire type", () => {
  for (const [number, size] of [[-1, 20], [0.5, 20], [0, 0], [0, NaN], [Infinity, 20], [0, 2_147_483_648], [2_147_483_647, 2]]) {
    assert.throws(() => page(number, size), RangeError);
  }
  assert.deepEqual(page(0, 20), { skip: 0, take: 20 });
});
