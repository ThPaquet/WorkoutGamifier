import React from 'react';
import { View, Text, StyleSheet } from 'react-native';

const ActionsScreen: React.FC = () => {
  return (
    <View style={styles.container}>
      <Text style={styles.title}>Actions</Text>
      <Text style={styles.subtitle}>Manage your point-earning activities</Text>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#f8fafc',
    padding: 20,
  },
  title: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#1f2937',
    marginBottom: 8,
  },
  subtitle: {
    fontSize: 16,
    color: '#6b7280',
    textAlign: 'center',
  },
});

export default ActionsScreen;