import { createRouter, createWebHistory } from "vue-router";
import DashboardLayout from "../layouts/DashboardLayout.vue"; // Kiểm tra kỹ đường dẫn file

const routes = [
  {
    path: "/",
    component: DashboardLayout, // THẰNG CHA (Chứa Sidebar/Header)
    redirect: "/dashboard",
    children: [
      {
        path: "dashboard",
        component: () => import("../views/Dashboard.vue") // THẰNG CON 1
      },
      {
        path: "holidays",
        component: () => import("../views/HolidayList.vue") // THẰNG CON 2
      }
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;