<<<<<<< HEAD
import { createRouter, createWebHistory } from 'vue-router'

import LeaveView from '../views/LeaveView.vue'

import LeaveDashboardView from '../views/LeaveDashboardView.vue'
import LeaveRequestFormView from '../views/LeaveRequestFormView.vue'

const routes = [
  {
    path: '/',
    redirect: '/leave' 
  },
  {
    path: '/leave-test', 
    name: 'LeaveTest',
    component: LeaveView
  },
  {
    path: '/leave',
    name: 'LeaveDashboard',
    component: LeaveDashboardView
  },
  {
    path: '/leave/request',
    name: 'LeaveRequest',
    component: LeaveRequestFormView
=======
import { createRouter, createWebHistory } from "vue-router"
import DashboardLayout from "../layouts/DashboardLayout.vue"

import Login from "../views/Login.vue"
import DashboardAdmin from "../views/DashboardAdmin.vue"
import DashboardManager from "../views/DashboardManager.vue"
import DashboardEmployee from "../views/DashboardEmployee.vue"
import LeaveRequest from "../views/LeaveRequest.vue" 
const routes = [
  {
    path: "/login",
    component: Login
  },
  {
    path: "/",
    component: DashboardLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: "admin",
        component: DashboardAdmin,
        meta: { role: "Admin" }
      },
      {
        path: "manager",
        component: DashboardManager,
        meta: { role: "Manager" }
      },
      {
        path: "employee",
        component: DashboardEmployee,
        meta: { role: "Employee" }
      },
      {
        path: "leave/new",          
        component: LeaveRequest,
        meta: { role: "Employee" }  
      }
    ]
>>>>>>> main
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token")
  const role = localStorage.getItem("role")

  if (to.matched.some(record => record.meta.requiresAuth) && !token) {
    return next("/login")
  }

  if (to.meta.role && to.meta.role !== role) {
    return next("/login")
  }

  next()
})

export default router