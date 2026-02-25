import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "/",
    redirect: "/holidays",
  },
  {
    path: "/holidays",
    component: () => import("../views/HolidayList.vue"),
  },
  {
    path: "/leave",
    component: {
      template: "<h2>Leave Page</h2>",
    },
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;