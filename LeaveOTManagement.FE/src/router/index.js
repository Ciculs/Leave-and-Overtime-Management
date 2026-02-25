import { createRouter, createWebHistory } from 'vue-router'
import LeaveView from '../views/LeaveView.vue'

const routes = [
  {
    path: '/leave',
    name: 'Leave',
    component: LeaveView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router