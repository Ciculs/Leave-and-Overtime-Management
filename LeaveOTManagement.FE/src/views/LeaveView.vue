<template>
  <div>
    <h2>Leave List</h2>

    <button @click="loadLeaves">Load Leaves</button>

    <ul>
      <li v-for="leave in leaves" :key="leave.id">
        {{ leave.reason }} - {{ leave.status }}
      </li>
    </ul>
  </div>
</template>

<script>
import { getLeaves } from "../services/leaveService";

export default {
  data() {
    return {
      leaves: []
    };
  },
  methods: {
    async loadLeaves() {
      try {
        const response = await getLeaves();
        this.leaves = response.data;
      } catch (error) {
        console.error("API Error:", error);
      }
    }
  }
};
</script>