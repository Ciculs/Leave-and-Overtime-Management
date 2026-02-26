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
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router